# Issue #30 — Commit A: financial navigation deduplication

Baseline: 4ac08cb521608bcdab00de0b0807bfbb85cdee7f (source identical to Issue29 d980be8 / RC1 SHA256 8d6ddb213fe69105bbbbc10b6d7746f36c8851912bebfcd5ebd002e643532d6b). Actual installed source identity is not independently available; no tag identity assumed.

Remove only the direct Management > Expenses & Bills link. Expenses remains under Finances & Accounting, including Equipment & Upgrades and all payments. Update Management actions, values, descriptions, Controller and Finances Back indices. No finance/equipment algorithms, save keys, point editors or business timing change. Later hub insertion is a separate commit.

Changed: GetMenuItems, ExecuteManagementSelection, NavigateBack, GetMenuItemValue, GetSelectedDescription. GetMenuTitle/ExecuteSelection require no positional changes here; page-based routing remains intact.

Manual QA (NOT RUN): select every Management row, return from Finances/Controller/Employees/Customers/Inventory, traverse Accounting > Expenses > Equipment and mechanic invoices and Back. Verify transfers still correspond to deposit/withdraw; browsing causes no transactions.
