# Flow: Purchase Requisition -> Purchase Order -> Goods Receipt -> Inventory

Diagram: [../diagrams/pr-po-goods-receipt-flow.png](../diagrams/pr-po-goods-receipt-flow.png) (source: [../diagrams/pr-po-goods-receipt-flow.mmd](../diagrams/pr-po-goods-receipt-flow.mmd))

## Detail per langkah

1. **Purchase Requisition (PR)** — Draft -> Submit -> melewati `Setting_ApprovalMatrices` -> Approved (atau Rejected). Setelah Approved, `LifecycleStatus = Open` dan qty-nya bisa mulai "dipesan" via PO.
2. **Purchase Order (PO)** — dibuat dari satu/lebih PR Approved (atau manual tanpa PR). Line PO menyimpan breakdown sumbernya di `Procurement_PurchaseOrderLineSources` (berapa qty dari PR line mana), supaya sisa qty PR yang belum dipesan bisa dihitung akurat (dan sekarang juga mengecualikan PO yang `Superseded`/`Cancelled` dari hitungan itu). PO juga lewat proses approval yang sama.
3. **Goods Receipt (GR)** — dibuat di modul Inventory, dengan field `PurchaseOrderId` (nullable) yang menghubungkannya ke PO. Saat GR di-*post/finalize*:
   - `GoodsReceiptUseCase.FinalizePostAsync` mengelompokkan baris GR per `ItemId`, lalu memanggil `IPurchaseOrderReceiptService.ApplyReceiptAsync` (kontrak lintas-modul di `Shared/Contracts/`, diimplementasi oleh Procurement).
   - Procurement mendistribusikan qty yang diterima ke line PO yang cocok (naikkan `ReceivedQuantity`), lalu set `LifecycleStatus` PO jadi `PartiallyReceived` atau `FullyReceived`.
   - Bersamaan itu, GR juga memposting stok ke `Inventory_Batches`, `Inventory_StockLedgers`, `Inventory_StockBalances` di modul Inventory — jadi barang benar-benar "masuk" ke inventory di titik ini.
4. Dari sini PO yang **Open/PartiallyReceived** masih bisa di-**Revise** (buat revisi baru, yang lama jadi `Superseded`), di-**Cancel** (kalau belum ada GR sama sekali), atau di-**Close** (menutup sisa yang tak akan dikirim, melepas sisa qty itu kembali ke PR).

## Catatan gap

~~Form Create/Edit Goods Receipt di UI belum punya dropdown untuk memilih Purchase Order~~ — sudah ditutup: GR Create/Edit sekarang punya dropdown Purchase Order (terfilter ke order `Approved` + `Open`/`PartiallyReceived`, ditambah order yang sedang ter-link walau sudah lewat filter itu), GR Detail menampilkan link balik ke PO-nya, dan PO Detail punya tombol "Create Goods Receipt" yang langsung membuka form GR dengan PO ter-pilih. Detail lengkap juga ada di [../modules/procurement.md](../modules/procurement.md).
