using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Madeline.ModMismatchFormatter.Patches;
using Xunit;

namespace ModMismatchFormatter.Tests;

/// <summary>
/// Exercises <see cref="CallInserter.InsertBefore"/> with real Harmony patching on a method shaped like
/// ScribeMetaHeaderUtility.WriteMetaHeader: if (EnterNode()) try { ... } finally { ExitNode(); }
/// </summary>
[Collection(nameof(CallInserterTests))] // Patches static state; keep these tests serial.
public class CallInserterTests
{
    private static readonly List<string> Calls = [];

    private static bool EnterNode()
    {
        Calls.Add("enter");
        return true;
    }

    private static void ExitNode() => Calls.Add("exit");

    private static void Inserted() => Calls.Add("inserted");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WriteHeader()
    {
        if (!EnterNode())
            return;

        try
        {
            Calls.Add("body");
        }
        finally
        {
            ExitNode();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void WriteHeaderThatThrows()
    {
        if (!EnterNode())
            return;

        try
        {
            throw new InvalidOperationException("boom");
        }
        finally
        {
            ExitNode();
        }
    }

    private static IEnumerable<CodeInstruction> InsertBeforeExitNode(IEnumerable<CodeInstruction> instructions) =>
        CallInserter.InsertBefore(instructions, Method(nameof(ExitNode)), Method(nameof(Inserted)), out _);

    private static MethodInfo Method(string name) => AccessTools.Method(typeof(CallInserterTests), name);

    private static void Patch(string original, string harmonyId)
    {
        new Harmony(harmonyId).Patch(Method(original), transpiler: new HarmonyMethod(Method(nameof(InsertBeforeExitNode))));
    }

    [Fact]
    public void Inserted_call_runs_inside_the_node_before_it_closes()
    {
        Patch(nameof(WriteHeader), "tests.write-header");
        Calls.Clear();

        WriteHeader();

        Assert.Equal(["enter", "body", "inserted", "exit"], Calls);
    }

    [Fact]
    public void Finally_block_still_runs_when_the_body_throws()
    {
        Patch(nameof(WriteHeaderThatThrows), "tests.write-header-throws");
        Calls.Clear();

        Assert.Throws<InvalidOperationException>(WriteHeaderThatThrows);
        Assert.Equal(["enter", "inserted", "exit"], Calls);
    }

    [Fact]
    public void Reports_when_the_target_call_is_missing()
    {
        var instructions = new List<CodeInstruction> { new(OpCodes.Nop), new(OpCodes.Ret) };

        var result = CallInserter.InsertBefore(instructions, Method(nameof(ExitNode)), Method(nameof(Inserted)), out bool inserted);

        Assert.False(inserted);
        Assert.Equal(2, result.Count);
    }
}
