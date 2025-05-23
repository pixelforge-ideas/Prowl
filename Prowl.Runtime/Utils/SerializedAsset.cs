﻿// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.IO;

using Prowl.Runtime.SceneManagement;
using Prowl.Echo;

namespace Prowl.Runtime.Utils;

public class SerializedAsset
{
    public EngineObject? Main;
    public List<EngineObject> SubAssets = new();

    [SerializeIgnore]
    public Guid Guid;

    public bool HasMain => Main != null;

    // Default constructor for serialization
    public SerializedAsset()
    {
    }

    public SerializedAsset(Guid assetGuid)
    {
        Guid = assetGuid;
    }

    public void SaveToFile(FileInfo file, out HashSet<Guid>? dependencies)
    {
        if (Main == null) throw new Exception("Asset does not have a main object.");

        file.Directory?.Create(); // Ensure the Directory exists
        SerializationContext ctx = new();
        var tag = Serializer.Serialize(this, ctx);
        dependencies = ctx.dependencies;

        using var stream = file.OpenWrite();
        using BinaryWriter writer = new(stream);
        tag.WriteToBinary(writer);
    }

    public void SaveToStream(Stream writer)
    {
        if (Main == null) throw new Exception("Asset does not have a main object.");

        var tag = Serializer.Serialize(this);
        using BinaryWriter binarywriter = new(writer);
        tag.WriteToBinary(binarywriter);
    }

    public static SerializedAsset FromSerializedAsset(string path)
    {
        using var stream = File.OpenRead(path);
        using BinaryReader reader = new(stream);
        var tag = EchoObject.ReadFromBinary(reader);
         
        try
        {
            var obj = Serializer.Deserialize<SerializedAsset>(tag);
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception("Failed to deserialize asset: " + path, e);
        }
    }

    public static SerializedAsset FromStream(Stream stream)
    {
        using BinaryReader reader = new(stream);
        var tag = EchoObject.ReadFromBinary(reader);

        try
        {
            var obj = Serializer.Deserialize<SerializedAsset>(tag);
            return obj;
        }
        catch (Exception e)
        {
            throw new Exception("Failed to deserialize asset from stream", e);
        }
    }

    public AssetRef<T> AddSubObject<T>(T obj) where T : EngineObject
    {
        if (obj == null) throw new Exception("Asset cannot be null");
        if (SubAssets.Contains(obj) || ReferenceEquals(Main, obj)) throw new Exception("Asset already contains this object: " + obj);
        obj.AssetID = Guid;
        obj.FileID = (ushort)(SubAssets.Count + 1);
        SubAssets.Add(obj);

        return new AssetRef<T>(obj);
    }

    public void SetMainObject(EngineObject obj)
    {
        if (obj == null) throw new Exception("Asset cannot be null");
        if (SubAssets.Contains(obj)) throw new Exception("Asset already contains this object: " + obj);
        obj.FileID = 0;
        Main = obj;
    }

    public void Destroy()
    {
        Main?.DestroyImmediate();
        foreach (var obj in SubAssets)
            obj.DestroyImmediate();
    }

    public EngineObject GetAsset(ushort fileID)
    {
        if (fileID == 0) return Main;
        return SubAssets[fileID - 1];
    }
}
