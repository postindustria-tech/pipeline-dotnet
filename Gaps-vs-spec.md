- Cancellation token
- Device detection pipeline builder - Removed from spec. Suggest marking obsolete. The benefit seems almost non existent and the downsides are considerable. 
(More code to maintain, more documentation, very difficult to use this with web integration, more complicated to migrate from this to more advanced scenarios)
- Get property directly from flow data by string - Removed from spec.
- .NET does not have 'GetDataTypeFromElement' - see pipeline-specification/features/access-to-results.md
- Builders 
  - Side-by-side generic class hierarchies of elements and builders creates a very confusing picture.
  - Default values are not defined in a consistent location. Mostly, this is done in builders. In some cases, doing this would not allow existing logic to function in exactly the same way. Suggest redesign to make this entirely consistent and the make values easier to find.
- .NET has an element that writes to evidence. This is incompatible with thread-safety.md#evidence
