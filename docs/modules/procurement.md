# Module Log: Procurement

> Not yet implemented. This file currently only records the cross-module dependency touchpoints with Inventory, captured while designing the Inventory module, so context isn't lost by the time Procurement is actually built. Full module log starts once implementation begins.

## Dependency on Inventory (recorded during Inventory design phase)
See [inventory.md](inventory.md#end-to-end-flow-cross-module-dependencies-annotated) for the full sequence diagram.

- Procurement owns Purchase Requisition/PO creation and its approval gate — entirely internal to Procurement, no Inventory involvement.
- When a Goods Receipt is posted (supplier delivery scanned in), Procurement calls **Inventory's** `IGoodsReceiptStockService.ReceiveStockAsync(itemId, batchInfo, qty, warehouseId)` (declared in `Shared/Contracts/`, implemented by Inventory). This is the only call Procurement makes into Inventory.
- Procurement must pass through whatever batch/legality metadata (batch code, SVLK/FSC document refs) it collected at receiving, since Inventory's `Inventory_Batches` table needs it and Procurement should not need to read/write Inventory's tables directly (module isolation).
- Landed cost inputs (freight/insurance/handling/duty) captured during Procurement's receiving step should also be passed through the same contract call (or a dedicated request DTO) so Inventory can populate `Inventory_LandedCostAllocations` — exact DTO shape to be finalized when Procurement is built.
