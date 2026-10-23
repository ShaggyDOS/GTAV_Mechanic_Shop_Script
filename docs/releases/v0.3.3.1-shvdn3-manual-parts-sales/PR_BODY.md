Implements #25 as a focused draft stacked on feature/shvdn3-27-repair-cost-coverage. Inventory → Parts Sales offers four explicit one-unit sales at the shared fixed acquisition estimate, protecting physical/background/remote repair reservations. One confirmation credits retail revenue once, independently from customer repair revenue, without changing supplier invoices or payroll.

A same-directory atomic save snapshot retains the original full payload and unknown keys, with two additive zero-default retail counters. A failed sale commit rolls back stock and all changed money. No NPCs, passive selling, manager procurement, placement changes or duplicate package removal.

Actual net48 Release build and all original 85 plus repair 32 plus sale 29 behavior cases (146 total) and 21 static checks pass. Production atomic helper fault-tests use synthetic files; Windows/GTA execution NOT RUN. Full diff, logs, source package receipt and Word/PDF documentation are in docs/releases/v0.3.3.1-shvdn3-manual-parts-sales.

DRAFT / CHATGPT REVIEW PENDING. Cost estimates do not invent historical lot prices. Foundation runtime-customer restoration remains a documented limitation. Crash cause UNATTRIBUTED; Enhanced runtime binding NOT VERIFIED. No QA R2/main merge or live GTA/save/settings edit.
