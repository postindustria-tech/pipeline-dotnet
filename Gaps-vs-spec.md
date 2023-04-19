- Flow data changes
  - Cancellation token - This was added to the spec in the spring 2023 re-write.
  - Get property value directly from flow data by string name - This was removed from spec during the spring 2023 re-write.
  - .NET does not have 'GetDataTypeFromElement' - see pipeline-specification/features/access-to-results.md
  - Evidence is not immutable. (This was added in the spring 2023 re-write to prevent additional complexity that would be 
    required to handle mutable evidence in some scenarios)
    - .NET has at least one element that writes to evidence (GetHighEntropyElement). This would need to change if immutable 
      evidence was implemented.
- Builders 
  - Side-by-side generic class hierarchies of elements and builders creates a very confusing picture.
  - Default values are not defined in a consistent location. Mostly, this is done in builders. In some cases, doing this 
    would not allow existing logic to function in exactly the same way. Suggest redesign to make this entirely consistent 
    and the make values easier to find.
  - The spec does not have an answer to these problems, but suggests they are avoided in future implementations. Updating 
    the older implementations to address them would also be good (but would break backwards compatibility)
  - Device detection pipeline builder - Removed from spec. There is discussion of this in the 'special builder' paragraph in 
    [reference implementation notes](https://github.com/51Degrees/specifications/blob/main/pipeline-specification/reference-implementation-notes.md#builders). 
    Suggest either marking it and related base classes obsolete or spending some effort to investigate how the downsides 
    could be mitigated. 
- Usage sharing - Java was updated with some changes in mid 2022. The spec includes these changes but they are not yet 
  implemented in .NET
  - CDATA wrapper should no longer be added to values
  - Characters that are invalid XML should be replaced with a substitute and 'escaped="true"' added to the attributes for 
    that element
  - Data is truncated if necessary and 'truncated' added to the attributes for that element.
  - 'BadSchema' flag is no longer needed (superseded by 'escaped=true')
- Data update - A general re-factor of the data update service is needed to remove complexity and align with the spec following
  changes made in the Spring 2023 spec re-write. 
  - Currently, when timer expires, the code checks local file, then checks URL. In the new spec, this logic would flow 
    differently.
  - The spec suggests preventing the user from creating an engine with an invalid update configuration. (E.g. by using a more 
    restrictive set of builder classes that constrain the options available to those that are valid)
  - Some terminology changes:
    - 'data file' becomes 'data source' - This is to prevent confusion when talking about a data 'file' that is not a file, 
      but a byte array in memory.
    - 'temp data file' becomes 'operational data file' - This is to better reflect the purpose and usage of this copy of the 
      data file.
- Device detection 
  - [Device detection on premise](https://github.com/51Degrees/specifications/blob/main/device-detection-specification/pipeline-elements/device-detection-on-premise.md#element-data) - 
    mentions additional complexity in the match metric accessors in Java and .NET intended to cope with having separate 
    engines for each component. This is no longer needed and could be removed.
