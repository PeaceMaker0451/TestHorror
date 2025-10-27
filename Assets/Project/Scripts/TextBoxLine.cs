using UnityEngine;

public readonly partial struct TextBoxLine
{
    public readonly string Head;
    public readonly string Text;

    public readonly AudioClip TextSound;
    public readonly Color TextColor;

    public readonly int TypingSpeed;
    public readonly int NewLineTypingSpeed;
    public readonly int PeriodTypingSpeed;
    public readonly int CommaTypingSpeed;
    public readonly int DashTypingSpeed;

    public TextBoxLine(string head, string text, Color textColor, int typingSpeed, int newLineTypingSpeed, int periodTypingSpeed, int commaTypingSpeed, int dashTypingSpeed, AudioClip textSound = null)
    {
        Head = head;
        Text = text;
        TextColor = textColor;
        TypingSpeed = typingSpeed;
        NewLineTypingSpeed = newLineTypingSpeed;
        PeriodTypingSpeed = periodTypingSpeed;
        CommaTypingSpeed = commaTypingSpeed;
        DashTypingSpeed = dashTypingSpeed;
        TextSound = textSound;
    }
}
