
import os
import sys

current = print(os.path.dirname(os.path.realpath(__file__)))
releasePath = os.path.abspath(f"{current}/../src/aspnetcore/artifacts/bin/Microsoft.AspNetCore.Server.Kestrel/Release/net8.0")
csProjPath = os.path.abspath(f"{current}/../src/Utopia.Core/")
files = os.listdir(releasePath)

dlls = []

for file in files:
    if not file.endswith(".dll"):
        continue

    dlls.append(file)

text = ""

for dll in dlls:
    text +=  \
f"""
<Reference Include="{dll}">
    <HintPath>{os.path.relpath(f"{releasePath}/{dll}",csProjPath)}</HintPath>
</Reference>
"""
    
print(f"get dlls:f{dlls}")

with open(f"{csProjPath}/Utopia.Core.csproj",mode="w+",encoding="utf-8") as fs:
    origin = fs.read()

    startSign = "<!-- Refer to dlls starts -->"
    endSign = "<!-- Refer to dlls ends -->"

    start = origin.find(startSign)
    end = origin.find(endSign)

    print("-------------")
    print(origin[0:start + len(startSign)])
    print("-------------")
    print(text)
    print("-------------")
    print(origin[end:])
    print("-------------")

    origin = origin[0:start + len(startSign)] + text + origin[end:]

    print(origin)
