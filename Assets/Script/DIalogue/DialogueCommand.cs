using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueCommand {
    void Execute(string[] parameters);
    void Draw();
}

[System.AttributeUsage(System.AttributeTargets.Class)]
public class DialogueAttribute : System.Attribute 
{
    public DialogueCommandType  CommandType { get; }
    public string               Color { get; set; }
    
    public DialogueAttribute(DialogueCommandType type) {
        this.CommandType = type;
    }
}

public enum DialogueCommandType {
    None,
    Dialogue,
}

public class DialogueCommand {

    [DialogueAttribute(DialogueCommandType.None)]
    public class Command_None : IDialogueCommand {
        public void Execute(string[] parameters) 
        {
            
        }

        public void Draw() 
        {

        }
    }

    [DialogueAttribute(DialogueCommandType.Dialogue, Color = "green")]
    public class Command_Dialogue : IDialogueCommand {
        public void Execute(string[] parameters) 
        {
            
        }

        public void Draw() 
        {
            Color currentColor = GUI.color;
            GUI.color = Color.green;
    /*
            bool selected = i == editObject._selectedPointIndex;
            if(selected)
                GUILayout.BeginHorizontal("box");*/

            GUILayout.Label("Test Label", GUILayout.Width(200f));

            if(GUILayout.Button("Button 1"));
            //    selectTrackPoint(i, MovementTrackDataEditObject.SelectInfo.Point);

            if(GUILayout.Button("Button 2"))
            {
                
            }

            GUI.color = currentColor;
        }
    }
}