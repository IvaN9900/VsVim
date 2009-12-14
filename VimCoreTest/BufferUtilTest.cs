﻿using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Vim;
using System.Windows.Input;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;

namespace VimCoreTest
{
    [TestFixture]
    public class BufferUtilTest
    {
        static string[] s_lines = new string[]
            {
                "summary description for this line",
                "some other line",
                "running out of things to make up"
            };

        ITextBuffer _buffer = null;

        [SetUp]
        public void Init()
        {
            Initialize(s_lines);
        }

        public void Initialize(params string[] lines)
        {
            _buffer = Utils.EditorUtil.CreateBuffer(lines);
        }

        [Test]
        public void AddLineBelow()
        {
            var line = _buffer.CurrentSnapshot.GetLineFromLineNumber(0);
            var newLine = BufferUtil.AddLineBelow(line);
            Assert.AreEqual(1, newLine.LineNumber);
            Assert.AreEqual(String.Empty, newLine.GetText());
           
        }

        [Test, Description("New line at end of buffer")]
        public void AddLineBelow2()
        {
            var line = _buffer.CurrentSnapshot.Lines.Last();
            var newLine = BufferUtil.AddLineBelow(line);
            Assert.IsTrue(String.IsNullOrEmpty(newLine.GetText()));
        }

        [Test, Description("Make sure the new is actually a newline")]
        public void AddLineBelow3()
        {
            Initialize("foo");
            BufferUtil.AddLineBelow(_buffer.CurrentSnapshot.GetLineFromLineNumber(0));
            Assert.AreEqual(Environment.NewLine, _buffer.CurrentSnapshot.GetLineFromLineNumber(0).GetLineBreakText());
            Assert.AreEqual(String.Empty, _buffer.CurrentSnapshot.GetLineFromLineNumber(1).GetLineBreakText());
        }

        [Test, Description("Make sure line inserted in the middle has correct text")]
        public void AddLineBelow4()
        {
            Initialize("foo", "bar");
            BufferUtil.AddLineBelow(_buffer.CurrentSnapshot.GetLineFromLineNumber(0));
            var count = _buffer.CurrentSnapshot.LineCount;
            foreach (var line in _buffer.CurrentSnapshot.Lines.Take(count-1))
            {
                Assert.AreEqual(Environment.NewLine, line.GetLineBreakText());
            }
        }

        [Test]
        public void AddLineBelow5()
        {
            Initialize("foo bar", "baz");
            BufferUtil.AddLineBelow(_buffer.CurrentSnapshot.GetLineFromLineNumber(0));
            var line = _buffer.CurrentSnapshot.GetLineFromLineNumber(0);
            Assert.AreEqual(Environment.NewLine, line.GetLineBreakText());
            Assert.AreEqual(2, line.LineBreakLength);
            Assert.AreEqual("foo bar", line.GetText());
            Assert.AreEqual("foo bar" + Environment.NewLine, line.GetTextIncludingLineBreak());

            line = _buffer.CurrentSnapshot.GetLineFromLineNumber(1);
            Assert.AreEqual(Environment.NewLine, line.GetLineBreakText());
            Assert.AreEqual(2, line.LineBreakLength);
            Assert.AreEqual(String.Empty, line.GetText());
            Assert.AreEqual(String.Empty + Environment.NewLine, line.GetTextIncludingLineBreak());

            line = _buffer.CurrentSnapshot.GetLineFromLineNumber(2);
            Assert.AreEqual(String.Empty, line.GetLineBreakText());
            Assert.AreEqual(0, line.LineBreakLength);
            Assert.AreEqual("baz", line.GetText());
            Assert.AreEqual("baz", line.GetTextIncludingLineBreak());
        }

        [Test]
        public void AddLineAbove1()
        {
            Initialize("foo");
            var newLine = BufferUtil.AddLineAbove(_buffer.CurrentSnapshot.GetLineFromLineNumber(0));
            Assert.AreEqual(0, newLine.LineNumber);
            Assert.AreEqual(2, _buffer.CurrentSnapshot.LineCount);
            Assert.AreEqual(String.Empty, newLine.GetText());
        }

        [Test]
        public void AddLineAbove2()
        {
            Initialize("bar", "baz");
            var newLine = BufferUtil.AddLineAbove(_buffer.CurrentSnapshot.GetLineFromLineNumber(1));
            Assert.AreEqual(String.Empty, newLine.GetText());
            var tss = _buffer.CurrentSnapshot;
            Assert.AreEqual("bar", tss.GetLineFromLineNumber(0).GetText());
            Assert.AreEqual("baz", tss.GetLineFromLineNumber(2).GetText());
        }

        [Test]
        public void DeleteSpan1()
        {
            Initialize("foo", "bar");
            var r = new Register('a');
            var span = new SnapshotSpan(_buffer.CurrentSnapshot, 0, 2);
            var tuple = Tuple.Create(span, MotionKind.Inclusive, OperationKind.CharacterWise);
            BufferUtil.DeleteSpan(tuple, r);
            Assert.AreEqual("fo", r.StringValue);
        }

        [Test]
        public void ShiftRight1()
        {
            Initialize("foo");
            var span = _buffer.CurrentSnapshot.GetLineFromLineNumber(0).Extent;
            BufferUtil.ShiftRight(span, 2);
            Assert.AreEqual("  foo", _buffer.CurrentSnapshot.GetLineFromLineNumber(0).GetText());
        }

        [Test, Description("Only shift whitespace")]
        public void ShiftLeft1()
        {
            Initialize("foo");
            var span = _buffer.CurrentSnapshot.GetLineFromLineNumber(0).Extent;
            BufferUtil.ShiftLeft(span, 2);
            Assert.AreEqual("foo", _buffer.CurrentSnapshot.GetLineFromLineNumber(0).GetText());
        }

        [Test, Description("Don't puke on an empty line")]
        public void ShiftLeft2()
        {
            Initialize("");
            var span = _buffer.CurrentSnapshot.GetLineFromLineNumber(0).Extent;
            BufferUtil.ShiftLeft(span, 2);
            Assert.AreEqual("", _buffer.CurrentSnapshot.GetLineFromLineNumber(0).GetText());
        }

        [Test]
        public void ShiftLeft3()
        {
            Initialize("  foo", "  bar");
            var span = new SnapshotSpan(
                _buffer.CurrentSnapshot.GetLineFromLineNumber(0).Start,
                _buffer.CurrentSnapshot.GetLineFromLineNumber(1).End);
            BufferUtil.ShiftLeft(span, 2);
            Assert.AreEqual("foo", _buffer.CurrentSnapshot.GetLineFromLineNumber(0).GetText());
            Assert.AreEqual("bar", _buffer.CurrentSnapshot.GetLineFromLineNumber(1).GetText());
        }

        [Test]
        public void ShiftLeft4()
        {
            Initialize("   foo");
            var span = _buffer.CurrentSnapshot.GetLineFromLineNumber(0).Extent;
            BufferUtil.ShiftLeft(span, 2);
            Assert.AreEqual(" foo", _buffer.CurrentSnapshot.GetLineFromLineNumber(0).GetText());
        }


    }
}
