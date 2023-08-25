# JTReader
JTReader as it's name, is designed for parsing and reading JT (Jupiter Tessellation) files. It is implemented purely in C# and is based on the .NET Framework 4.7.2

## Project and Readme still WIP

# JT file differences with document
__YES__, the official document has some critical error in it.  
I don't remeber all of it, almost of them it's easy to debug.  
So I just write some tough ones.  
1. First one is that with version v10.\*. When you decode Int32CDP with Arithmetic Codec type. In preveris version, you can directly read out of band values, if there is no oob valuse, you will get zero. But in v10.\* you need to check `Probability Context` to know if you need to read oob value.
```cs
    if(int32ProbabilityContexts.hasOutOfBandValues)
        outOfBandValues = DecodeBytes(data);
```
2. 
