using System.Collections;
using UnityEngine;

public interface IPoolableObject
{
    void New();
    void Respawn();
}