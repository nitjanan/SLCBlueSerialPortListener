-- คิวรอส่งข้อมูลชั่งขึ้นตาราง mirror บน web app (ใช้สำหรับกู้ข้อมูลเมื่อ local DB เสีย เช่นฟ้าผ่า)
-- แยกจากการอัพโหลด weight ตอนเย็น (ข้อมูลจริงที่พนักงานตรวจแล้ว) ไม่ไปปนกับตาราง weight บน server
-- ตัวส่งอยู่ที่ WeightMirrorSync.cs ; รันครั้งเดียวตอนเปิดโปรแกรมผ่าน MigrationRunner
-- ต้องใช้กับ PostgreSQL 9.5 ได้ (ไม่มี ADD COLUMN IF NOT EXISTS)

CREATE TABLE IF NOT EXISTS sync_outbox (
    id          bigserial   PRIMARY KEY,
    table_name  text        NOT NULL,
    row_id      text        NOT NULL,
    action      char(1)     NOT NULL,          -- I = เพิ่ม, U = แก้ไข, D = ลบ
    payload     jsonb,                          -- ข้อมูลทั้งแถว ณ เวลาล่าสุด
    rev         integer     NOT NULL DEFAULT 0, -- เพิ่มทุกครั้งที่แถวเดิมถูกแก้ซ้ำก่อนส่ง กันการ mark ว่าส่งแล้วทับข้อมูลใหม่
    created_at  timestamptz NOT NULL DEFAULT now(),
    sent_at     timestamptz,                    -- null = ยังไม่ได้ส่ง
    attempts    integer     NOT NULL DEFAULT 0,
    last_error  text
);

-- แถวที่ยังไม่ส่งมีได้แถวเดียวต่อรายการ แก้หลายรอบก่อนส่งจะยุบเหลือเวอร์ชันล่าสุด
CREATE UNIQUE INDEX IF NOT EXISTS sync_outbox_pending_uq
    ON sync_outbox (table_name, row_id) WHERE sent_at IS NULL;

CREATE INDEX IF NOT EXISTS sync_outbox_sent_at_idx
    ON sync_outbox (sent_at) WHERE sent_at IS NOT NULL;

CREATE OR REPLACE FUNCTION slc_weight_mirror_enqueue() RETURNS trigger
LANGUAGE plpgsql AS $$
DECLARE
    r   record;
    act char(1);
BEGIN
    IF TG_OP = 'DELETE' THEN
        r := OLD;
        act := 'D';
    ELSE
        r := NEW;
        act := CASE WHEN TG_OP = 'INSERT' THEN 'I' ELSE 'U' END;
    END IF;

    -- เก็บเฉพาะรายการล่าสุด (ยังไม่ได้อัพโหลดตอนเย็น) ไม่ให้การดาวน์โหลดการตั้งค่า
    -- ที่ UPDATE ชื่อลูกค้า/โรงโม่ ย้อนหลังทั้งตาราง ทำให้คิวมีเป็นหมื่นแถว
    IF r.weight_id IS NULL OR (r."วันที่" IS NOT NULL AND r."วันที่" < current_date - 7) THEN
        RETURN NULL;
    END IF;

    -- คิวเสียต้องไม่ทำให้การบันทึกน้ำหนักล้มเหลว
    BEGIN
        INSERT INTO sync_outbox (table_name, row_id, action, payload)
        VALUES (TG_TABLE_NAME, r.weight_id::text, act, row_to_json(r)::jsonb)
        ON CONFLICT (table_name, row_id) WHERE sent_at IS NULL
        DO UPDATE SET
            -- เพิ่มแล้วแก้ก่อนส่ง ยังนับเป็นการเพิ่ม ; ลบเมื่อไหร่ก็เป็นการลบ
            action     = CASE WHEN sync_outbox.action = 'I' AND EXCLUDED.action <> 'D' THEN 'I'
                              ELSE EXCLUDED.action END,
            payload    = EXCLUDED.payload,
            rev        = sync_outbox.rev + 1,
            created_at = now();
    EXCEPTION WHEN OTHERS THEN
        RAISE WARNING 'slc_weight_mirror_enqueue: %', SQLERRM;
    END;

    RETURN NULL;
END
$$;

DROP TRIGGER IF EXISTS slc_weight_mirror_ins_del ON weight;
CREATE TRIGGER slc_weight_mirror_ins_del
    AFTER INSERT OR DELETE ON weight
    FOR EACH ROW EXECUTE PROCEDURE slc_weight_mirror_enqueue();

-- UPDATE ที่ค่าไม่เปลี่ยนจริง (เช่นดาวน์โหลดข้อมูลเดิมทับ) ไม่ต้องเข้าคิว
DROP TRIGGER IF EXISTS slc_weight_mirror_upd ON weight;
CREATE TRIGGER slc_weight_mirror_upd
    AFTER UPDATE ON weight
    FOR EACH ROW WHEN (OLD.* IS DISTINCT FROM NEW.*)
    EXECUTE PROCEDURE slc_weight_mirror_enqueue();
