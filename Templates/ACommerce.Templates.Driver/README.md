Driver UI template
===================

This template provides a small scaffold for Driver-facing UI components and layouts.

How to use
----------
- Add this project to the solution (optional but recommended) so it builds with the rest of the repo.
- Consume the template from a Blazor shell project via a ProjectReference:

  <ProjectReference Include="..\\Templates\\ACommerce.Templates.Driver\\ACommerce.Templates.Driver.csproj" />

- Use `DriverLayout` as your App layout or copy components into an app-specific Shared project.

Next steps (recommended):
- Add the project to `ACommerce.Libraries.sln` so builds pick it up automatically.
- Move any Rukkab-specific shared components into this template if they are intended to be reused across Driver apps.
