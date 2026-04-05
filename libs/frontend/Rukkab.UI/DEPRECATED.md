This library `libs/frontend/Rukkab.UI` is deprecated.

Reason:
- The Rukkab UI components were moved into centralized Template and Shared projects under `Templates/ACommerce.Templates.Driver` and `Templates/ACommerce.Templates.Customer` to follow repository conventions (thin app shells + reusable templates).

Actions you can take:
- Update projects to reference `Apps/Rukkab/Shared/Rukkab.Shared.Customer` or `Apps/Rukkab/Shared/Rukkab.Shared.Driver` as appropriate.
- Remove this folder after verifying all references have switched.

If you need help migrating remaining references, open an issue or contact the maintainer.
