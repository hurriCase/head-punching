using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Source.Scripts.Gameplay.Gloves
{
    internal interface IPunchService
    {
        UniTask ExecutePunch(Transform target, Vector3 startPoint, Vector3 endPoint);
    }
}