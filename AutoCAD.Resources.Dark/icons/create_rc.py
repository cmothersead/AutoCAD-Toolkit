import os
from pathlib import Path

def getline(filepath: Path):
    return "ICA_" + os.path.splitext(os.path.relpath(filepath))[0].replace("\\","_").upper() + "\tRCDATA\t\"" + os.path.relpath(str(filepath), "../").replace("\\","/") + "\"\n"

def getlines(directory):
    output = ""
    for fname in os.listdir(directory):
        filepath = Path(str(directory) + os.sep + fname)
        if os.path.isfile(filepath):
            if filepath.suffix == ".ico":
                output += getline(filepath)
        else:
            output += getlines(filepath)
    return output

resources = open("../AutoCAD.Resources.Dark.rc", "w")
directory = os.getcwd()
resources.write(getlines(directory))
resources.close()