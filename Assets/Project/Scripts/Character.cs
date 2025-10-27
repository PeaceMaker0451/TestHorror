using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Character")]
public class Character: ScriptableObject
{
    public string Name;
    public Color TextColor;
    public string TextPrefix;
    public string TextPostfix;

    public AudioClip TextSound;

    public int TypingSpeed = 35;
    public int NewLineTypingSpeed = 500;
    public int CommaTypingSpeed = 200;
    public int PeriodTypingSpeed = 80;
    public int DashTypingSpeed = 200;

    public TextBoxLine PersonalizeLine(string line)
    {
        string text = $"{TextPrefix}{line}{TextPostfix}";

        return new TextBoxLine(Name, text, TextColor,
            TypingSpeed, NewLineTypingSpeed, PeriodTypingSpeed, CommaTypingSpeed, DashTypingSpeed, TextSound);
    }

    public List<TextBoxLine> PersonalizeLines(IEnumerable<string> phrases)
    {
        List<TextBoxLine> lines = new List<TextBoxLine>();

        if (phrases == null || phrases.Count() == 0) return lines;

        foreach (string phrase in phrases)
        {
            lines.Add(PersonalizeLine(phrase));
        }

        return lines;
    }
}