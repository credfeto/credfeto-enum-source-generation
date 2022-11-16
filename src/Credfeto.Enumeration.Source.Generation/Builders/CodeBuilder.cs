using System;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Credfeto.Enumeration.Source.Generation.Builders;

public sealed class CodeBuilder
{
    private readonly StringBuilder _stringBuilder = new();

    private int _indent;

    public SourceText Text => SourceText.From(this._stringBuilder.ToString(), encoding: Encoding.UTF8);

    public CodeBuilder AppendBlankLine()
    {
        this._stringBuilder.AppendLine();

        return this;
    }

    public CodeBuilder AppendLine(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return this.AppendBlankLine();
        }

        this._stringBuilder.Append(this.IndentCharacters())
            .AppendLine(text);

        return this;
    }

    public IDisposable StartBlock(string text)
    {
        return this.StartBlock(text: text, start: "{", end: "}");
    }

    public IDisposable StartBlock(string text, string start, string end)
    {
        this.AppendLine(text);

        return new Indent(this, start: start, end: end);
    }

    private string IndentCharacters()
    {
        int indentCharacters = 4 * this._indent;

        return string.Empty.PadLeft(indentCharacters);
    }

    private sealed class Indent : IDisposable
    {
        private readonly CodeBuilder _builder;
        private readonly string _end;

        public Indent(CodeBuilder builder, string start, string end)
        {
            this._builder = builder;
            this._end = end;

            this._builder.AppendLine(start);
            ++this._builder._indent;
        }

        public void Dispose()
        {
            --this._builder._indent;
            this._builder.AppendLine(this._end);
        }
    }
}