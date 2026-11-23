# Weight mirror API (ฝั่ง web app)

ตาชั่งส่งข้อมูลชั่งขึ้นมาตลอดวันเพื่อใช้กู้ข้อมูลเมื่อ local DB เสีย
**ไม่ใช่**ข้อมูลจริง ข้อมูลจริงยังมาจากการอัพโหลดตอนเย็น (`/weight/api/create/`) เหมือนเดิม
ตัวส่งอยู่ที่ `SerialPortListener/WeightMirrorSync.cs`

## Endpoint

`POST {base_api id=1 url}/weightmirror/api/bulk/`

- Auth: `Authorization: Bearer <JWT>` (จาก `/jwt/create/` เดิม) ตาชั่งเก็บ token ไว้ใช้ซ้ำ และจะ login ใหม่เมื่อได้ 401
- Body: `application/json; charset=utf-8` และส่งแบบ **`Content-Encoding: gzip`**
  - ถ้า server ตอบ **415** ตาชั่งจะส่งแบบไม่บีบอัดแทนไปจนกว่าจะปิดโปรแกรม
  - Django ไม่คลาย gzip ของ request ให้เอง ต้องมี middleware (ดูด้านล่าง)
- ตอบ 2xx = รับทั้งชุดแล้ว ; ตอบอย่างอื่น = ตาชั่งส่งทั้งชุดซ้ำรอบหน้า (ทุก 3 นาที)

```json
{
  "comp_code": "<base_api id=1 comp_code>",
  "mine_code": "<base_api id=2 comp_code>",
  "station_code": "<base_weight_station id=1 code>",
  "items": [
    {
      "table_name": "weight",
      "row_id": "12345",
      "action": "I",
      "rev": 0,
      "queued_at": "2026-11-23T08:38:48+07",
      "payload": { "weight_id": 12345, "วันที่": "2026-11-23", "...": "ทุก column ของแถว" }
    }
  ]
}
```

- `action`: `I` เพิ่ม, `U` แก้ไข, `D` ลบ (payload = ข้อมูลก่อนลบ)
- ส่งได้สูงสุด 200 items ต่อ request
- ต้อง **upsert** และส่งซ้ำได้ไม่เกิดรายการซ้ำ (idempotent) เพราะตาชั่งส่งซ้ำได้เสมอ
- `station_code` แยกตาชั่งที่ใช้ `comp_code` เดียวกัน (เช่น Blue/Pink) ตอนกู้ต้องเลือกให้ถูกตัว
- ตัวเลขทศนิยมต้องเก็บแบบไม่เปลี่ยนรูป (`12.340` ไม่ใช่ `12.34`) ฝั่ง Django ใช้ `DecimalJSONParser`

## ฝั่ง Django (ทำแล้ว ที่ djangoweight branch `add/weight-mirror`)

- Model `WeightMirror` (ตาราง `weight_mirror`) unique (`comp_code`, `table_name`, `row_id`) ใน `weightapp/models.py`
- `weightapp/views_weight_mirror.py`
  - `POST weightmirror/api/bulk/` : upsert ทั้งชุดด้วย `bulk_create(update_conflicts=True)` query เดียว
    (MySQL/MariaDB = `ON DUPLICATE KEY UPDATE` ห้ามใส่ `unique_fields`)
  - แถวที่ผิดรูปแบบจะถูกข้ามและตอบ 200 พร้อม `skipped` ไม่งั้นตาชั่งส่งชุดเดิมซ้ำไม่จบ
  - `GET weightmirror/api/weight/?station_code=&ids=1,2,3` : `weight` ที่อัพโหลดแล้วของตาชั่งนี้ตาม `weight_id`
    (ตอบเฉพาะที่พบ ไม่เกิน 500 ids ต่อครั้ง) ตาชั่งใช้เช็คสำเนาก่อนเติมตอนกู้
  - `GET weightmirror/api/stations/?station_code=` : สรุปสำเนาของรหัสตาชั่งนี้ แยกตาม `comp_code`/`mine_code`
    (จำนวน, วันที่ล่าสุด) ; `station_code` บังคับ ค้นด้วย `comp_code` อย่างเดียวไม่ได้
  - `GET weightmirror/api/list/?station_code=&comp_code=&mine_code=&table_name=weight&date_from=YYYY-MM-DD&page=` :
    ดึงกลับไปกู้ ตอบ `{results, next, count}` ทีละ 500 แถว ; `station_code` บังคับ ที่เหลือเป็นตัวกรองเสริม
- `weightapp/middleware.py` `GzipRequestMiddleware` คลาย gzip (จำกัดขนาดที่คลายแล้ว 20 MB)
- `python manage.py cleanup_weight_mirror --days 60` ตั้ง Task Scheduler วันละครั้ง
- หน้า admin ดูอย่างเดียว

## กู้ข้อมูล (ฝั่งโปรแกรม)

หน้า GUI / ซิงค์ข้อมูล > "กู้รายการชั่งจากเซิร์ฟเวอร์" (`ucBackup.btRestoreMirror` -> `RestoreMirrorForm` -> `WeightMirrorRestore.cs`)

1. ใช้รหัสตาชั่งของเครื่องนี้ (`base_weight_station.code` ของ `base_weight_station_id = 1`) ไม่ใช้ `comp_code`
   ถ้าเครื่องนี้ไม่มีรหัสตาชั่งจะหยุดและแจ้งให้ตั้งก่อน
2. หน้าต่างเลือกวันที่เริ่มต้น (ค่าเริ่มต้น 7 วันก่อน) และสำเนา (บริษัท/เหมือง) หรือ "ไม่ใช้สำเนา" ; ไม่มีสำเนาบน server ก็ยังกู้ได้
   เลือก "ไม่ใช้สำเนา" วันที่เริ่มจะถูกตั้งเป็นวันแรกที่มีรายการชั่งของตาชั่งนี้บน server ให้อัตโนมัติ
   (`weight_first_date` ใน `GET weightmirror/api/stations/` = `Min(Weight.date)` ของ `bws = station_code`)
   รายการชั่งดึงทีละเดือน (endpoint between/bws ไม่แบ่งหน้า) ปุ่มแสดงเดือนที่กำลังดึง
3. ดึงครบทั้งสองแหล่งก่อน แล้วเขียนลงเครื่องใน transaction เดียว (ดึงพลาด = ไม่มีอะไรถูกเขียน) ตามลำดับ :
   1. **weight ที่อัพโหลดแล้ว** `GET weight/api/between/bws/{วันที่เริ่ม}/{วันนี้}/{รหัสตาชั่ง}/`
      แปลงชื่อ field กลับเป็น column ในเครื่อง (กลับด้านจาก `GetWeightJsonDataForDate`) เวลา `HH:MM:SS` -> `HH:mm`
      **ยึดข้อมูลจาก server เป็นหลัก** ใช้ทั้งแถวตามที่ server มี ไม่ผสมกับสำเนา
      (column ที่ไม่ได้อัพโหลด และค่าว่างที่ server เก็บเป็น null จะเป็น null)
   2. **สำเนา weight_mirror** เติมเฉพาะรายการที่ server ไม่มี (ยังไม่ได้อัพโหลดตอนเย็น) ไม่เอารายการที่ถูกลบกลับมา
      สำเนาที่ `weight_id` ไม่อยู่ในช่วงวันที่ที่ดึง (เช่นวันที่ถูกแก้บนเว็บ) ถามซ้ำที่ `GET weightmirror/api/weight/?station_code=&ids=`
      ถ้ามีบน server ใช้ของ server แทน
   - ผล : กด "ไม่ใช้สำเนา" ก่อนแล้วกดสำเนา หรือกดสำเนาก่อนแล้วกด "ไม่ใช้สำเนา" ได้ข้อมูลเหมือนกันทุก column
4. เติมเฉพาะรายการที่ไม่มีในเครื่อง (`ON CONFLICT (weight_id) DO NOTHING`) ไม่ทับรายการเดิม
5. ใช้เฉพาะ column ที่มีในตารางเครื่องนี้ แต่ละค่า `CAST` เป็นชนิดของ column ; แถวที่เสียไม่ทำให้แถวอื่นกู้ไม่ได้ (SAVEPOINT)
6. หลังกู้เสร็จรัน `ALTER SEQUENCE weight_weight_id_seq RESTART WITH (max(weight_id) + 1)` อัตโนมัติ (ทุกครั้ง แม้ sequence จะสูงกว่าอยู่แล้ว)
   และลบรายการที่เพิ่งกู้ออกจากคิว `sync_outbox` ไม่ส่งซ้ำ
7. ตั้ง `seq_doc_num.run_number` = `เลขที่เอกสาร` ของ `weight_id` ล่าสุด (ปี = ปีของ `วันที่` รายการนั้น จัดรูปแบบแบบ MainForm
   `ToString("yyyy")` ; ไม่มีแถวของปีนั้นจะเพิ่มให้ ; เลขที่เอกสารว่าง/ไม่ใช่ตัวเลขจะไม่แตะ) ใบถัดไป = เลขนี้ + 1

ทดสอบจำลองข้อมูลหายบนฐานจริง : ห้าม `DELETE FROM weight` ตรงๆ เพราะ trigger จะส่ง `D` ขึ้น mirror แล้วสำเนาจะถูกนับว่าลบไปแล้ว
(ให้ `ALTER TABLE weight DISABLE TRIGGER USER` ก่อนลบ หรือใช้ฐานทดสอบตาม `SLC_ODBC_EXTRA` ด้านล่าง)

**ต้องกู้ก่อนเริ่มชั่งรายการแรก** ถ้าชั่งไปก่อนบน DB ที่ sequence เริ่มนับใหม่ รายการใหม่จะไปทับสำเนาของ `weight_id` เดิมบน server

ทดสอบโดยไม่แตะฐานจริง : ตั้ง `SLC_ODBC_EXTRA=Database=<ฐานทดสอบ>` ก่อนเปิดโปรแกรม ค่านี้จะทับค่า Database ของ DSN `PostgreSQLM`
