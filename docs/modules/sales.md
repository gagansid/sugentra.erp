# Module Log: Sales & Export Order

> Not yet implemented. This file currently only records the cross-module dependency touchpoints with Inventory, captured while designing the Inventory module, so context isn't lost by the time Sales is actually built. Full module log starts once implementation begins.

## Dependency on Inventory (recorded during Inventory design phase)
See [inventory.md](inventory.md#end-to-end-flow-cross-module-dependencies-annotated) for the full sequence diagram.

- When a Sales/Export Order is confirmed, Sales calls **Inventory's** `IInventoryReservationService.IsStockAvailableAsync(itemId, qty, warehouseId)` then `ReserveStockAsync(...)` to lock the finished-goods stock against that order — Inventory owns the reservation state (`QuantityReserved` on `Inventory_StockBalances`), Sales only calls the contract.
- Finance's release-of-goods approval (payment/LC verification) is a Sales/Finance concern, not Inventory's — Inventory is only told to proceed once Sales calls `ReleaseReservationAsync` after that approval clears.
- After release, Inventory independently manages the fumigation/quarantine hold (`QuarantineHolds`, `HoldReason = FumigationPending`) and the final stock cut (`StockLedger(Shipped)`) on container departure — Sales does not need to poll or drive that state, it only needs the "released" signal to proceed with export documentation triggers.
- Sales pushes final shipment quantity/dimensions/weight to Export Documentation — that data originates from Inventory (batch/stock records), so Sales should read it from Inventory's contract rather than duplicating it in Sales' own tables.
