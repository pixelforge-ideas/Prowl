﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Prowl.Runtime;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets;

[Importer("CSharpIcon.png", typeof(MonoScript), ".cs")]
public class MonoScriptImporter : ScriptedImporter
{
    public override void Import(SerializedAsset ctx, FileInfo assetPath)
    {
        ctx.SetMainObject(new MonoScript());

        Program.RegisterReloadOfExternalAssemblies();
    }
}
