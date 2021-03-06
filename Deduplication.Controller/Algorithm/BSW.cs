﻿using Deduplication.Controller.Extensions;
using Deduplication.Model.DTO;
using System;
using System.Collections.Generic;

namespace Deduplication.Controller.Algorithm
{
    internal class BSW : DeduplicationAlgorithm
    {
        public int D { get; set; }
        public int R { get; set; }

        public int MinT { get; set; }

        public BSW(int d, int r, int minT, Action<ProgressInfo, string> updateProgress = null) : base(updateProgress)
        {
            D = d;
            R = r;
            MinT = minT;
        }

        public override IEnumerable<Chunk> Chunk(byte[] bytes)
        {
            var sha256Str = GetSHA256Str(bytes);
            HashSet<Chunk> chunks = new HashSet<Chunk>();

            UpdateChunkingProgress("Start chuncking", 0, bytes.Length);
            for (long outset = 0, boundary = 0; boundary < bytes.Length; boundary++)
            {
                var padding = boundary + 1;
                var scope = padding - outset;
                if (scope >= MinT || (scope > 0 && padding == bytes.Length))
                {
                    var piece = bytes.SubArray(outset, scope);
                    var f = piece.GetHashCode();
                    if (f % D == R || padding == bytes.Length)
                    {
                        var chunk = new Chunk() {
                            Id = GetSHA256Str(piece),
                            Bytes = piece
                        };

                        chunks.Add(chunk);
                        UpdateChunkingProgress("Current break point", boundary);

                        outset = padding;
                    }
                }
            }
            UpdateChunkingProgress("Finished", bytes.Length, bytes.Length);

            return chunks;
        }
    }
}
