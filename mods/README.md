csproj file is there just so visual studio know how to compile mod, when published source code is zipped not dll
mod manager will compile sources at startup;  reasoning for this is that mod author cannot add 'extra' functionality to mod (like keylogger, etc.) - anybody can review mod source 


definition json is automatically upadated after build:
- version: uses AssemblyVersion
- resources: reads EmbeddedResource collections
- requires: reads from project ModReference

example mods:
- SimpleMod: basic example of mod; it includes optional IHarmonyPlugin and ITopRightButtonPlugin markers that enhance mod functionaliry by injecting manager code into OnIsEnabledChanged method
- SecondMod: ecample how to use method from another mod

