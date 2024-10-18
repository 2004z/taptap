using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MessageSender 
{
    public abstract void SendMessage(float distance, GameObject recipient);
}
