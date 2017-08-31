using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Reveal Text", 16)]
public class RevealText : BaseMeshEffect
{
    #region Replicate internal UnityEngine.UI classes

    // The MIT License (MIT)
    // Copyright (c) 2014-2015, Unity Technologies
    // Permission is hereby granted, free of charge, to any person obtaining a copy
    // of this software and associated documentation files (the "Software"), to deal
    // in the Software without restriction, including without limitation the rights
    // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    // copies of the Software, and to permit persons to whom the Software is
    // furnished to do so, subject to the following conditions:
    // The above copyright notice and this permission notice shall be included in
    // all copies or substantial portions of the Software.
    // Source: https://bitbucket.org/Unity-Technologies/ui/src/378bd9240df3/UnityEngine.UI/UI/Core/Utility/?at=5.6

    private class ObjectPool<T> where T : new()
    {
        public int CountAll { get; private set; }
        public int CountActive { get { return CountAll - CountInactive; } }
        public int CountInactive { get { return stack.Count; } }

        private readonly Stack<T> stack = new Stack<T>();
        private readonly UnityAction<T> actionOnGet;
        private readonly UnityAction<T> actionOnRelease;

        public ObjectPool (UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            this.actionOnGet = actionOnGet;
            this.actionOnRelease = actionOnRelease;
        }

        public T Get ()
        {
            T element;
            if (stack.Count == 0)
            {
                element = new T();
                CountAll++;
            }
            else
            {
                element = stack.Pop();
            }
            if (actionOnGet != null)
                actionOnGet(element);
            return element;
        }

        public void Release (T element)
        {
            if (stack.Count > 0 && ReferenceEquals(stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            if (actionOnRelease != null)
                actionOnRelease(element);
            stack.Push(element);
        }
    }

    private static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> listPool = new ObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get ()
        {
            return listPool.Get();
        }

        public static void Release (List<T> toRelease)
        {
            listPool.Release(toRelease);
        }
    }
    #endregion

    public int MaxFadeInterval = 100;

    private int revealStartIndex;
    private int revealEndIndex;
    private int revealInterval;
    private float revealStartTime;
    private float revealEndTime;
    private float revealDuration;

    [ContextMenu("Reveal")]
    public void Test ()
    {
        Reveal(0, 1598, 10f);
    }

    protected override void Awake ()
    {
        base.Awake();
        revealEndTime = float.NegativeInfinity;
    }

    private void Update ()
    {
        if (ShouldUpdateMesh()) 
            graphic.SetVerticesDirty();
    }

    public void Reveal (int start, int end, float time)
    {
        revealStartIndex = start;
        revealEndIndex = end;
        revealInterval = revealEndIndex - revealStartIndex;
        revealDuration = time;
        revealStartTime = Time.time;
        revealEndTime = revealStartTime + time;

        graphic.SetVerticesDirty();
    }

    public override void ModifyMesh (VertexHelper vertexHelper)
    {
        if (!ShouldUpdateMesh()) return;

        var vertexStream = ListPool<UIVertex>.Get();
        vertexHelper.GetUIVertexStream(vertexStream);

        ApplyOpacity(vertexStream);

        vertexHelper.Clear();
        vertexHelper.AddUIVertexTriangleStream(vertexStream);
        ListPool<UIVertex>.Release(vertexStream);
    }

    private void ApplyOpacity (List<UIVertex> verts)
    {
        if (verts == null || verts.Count == 0) return;

        Debug.Assert(revealStartIndex >= 0 &&
            revealStartIndex <= revealEndIndex &&
            verts.Count > revealEndIndex);

        UIVertex uiVertex;

        var revealProgress = (Time.time - revealStartTime) / revealDuration;
        var fadeStartIndex = revealStartIndex + revealInterval * revealProgress;
        var fadeInterval = Mathf.Min(MaxFadeInterval, revealEndIndex - fadeStartIndex);

        for (int i = 0; i < verts.Count; ++i)
        {
            uiVertex = verts[i];

            if (i <= fadeStartIndex)
                uiVertex.color.a = 255;
            else if (i > revealEndIndex)
                uiVertex.color.a = 0;
            else
            {
                var fadeProgress = 1f - (i - fadeStartIndex) / fadeInterval;
                uiVertex.color.a = (byte)Mathf.Lerp(0, 255, fadeProgress);
            }

            verts[i] = uiVertex;
        }
    }

    private bool ShouldUpdateMesh ()
    {
        return IsActive() && Time.time <= revealEndTime;
    }
}
