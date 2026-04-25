ExeRedirector
=========

ExeRedirector is a C# application to open files in applications based on the file path. This is intended to be used for file-types with no set standard (e.g. SHP) where different applications can edit different flavours of the file type.

## Usage

In Windows, set the extension you want to redirect, such as `.shp`, to open with `ExeRedirector.exe`.

Create `ExeRedirector.json` next to `ExeRedirector.exe`:

```json
[
  {
    "path": "C:\\game1",
    "filetype": "SHP",
    "app": "C:\\apps\\game1-handler.exe"
  },
  {
    "path": "C:\\game2",
    "filetype": ".shp",
    "app": "C:\\apps\\game2-handler.exe",
    "arguments": ["/open"]
  }
]
```

When Windows opens `C:\game1\file.shp`, `ExeRedirector` starts:

```text
C:\apps\game1-handler.exe C:\game1\file.shp
```

The longest matching `path` wins, so a mapping for `C:\games\game1\mods` is chosen before `C:\games\game1`.

`filetype` is required for each mapping. It can be written as `SHP`, `.shp`, or `*.shp`.

If no configured mapping matches the file path and extension, ExeRedirector opens the normal Windows "Open with" dialog so you can pick an application manually.

## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/exeredirector

# Go into the repository
$ cd src

# Build  the app
$ dotnet build
```

## Licencing

ExeRedirector is licenced under the MIT License. Full licence details are available in licence.md