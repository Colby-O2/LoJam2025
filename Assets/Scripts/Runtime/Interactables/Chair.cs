using LJ2025.Player;
using System.Collections;
using UnityEngine;

namespace LJ2025
{
    public class Chair : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Transform _exitLocation;
        [SerializeField] private SphereCollider _boundingRadius;

        public Transform ExitLocation() => _exitLocation;
        public Transform TargetLocation() => _target;
        
        public string GetHintName() => "Chair";
        public string GetHintAction() => "Sit";
        public bool IsInteractable() => !LJ2025GameManager.Player.IsInChair(this);
        public SphereCollider BoundingRadius() => _boundingRadius;

        public IEnumerator RotateAndDetachHead(Player.Controller player, Transform chairTarget, float duration = 0.5f)
        {
            Transform playerTrans = player.transform;

            Vector3 directionAwayFromChair = playerTrans.position - chairTarget.position;
            directionAwayFromChair.y = 0; 
            Quaternion endRot = Quaternion.LookRotation(directionAwayFromChair);

            Quaternion startRot = playerTrans.rotation;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                playerTrans.rotation = Quaternion.Slerp(startRot, endRot, t);
                yield return null;
            }

            playerTrans.rotation = endRot;

            player.DetachHead(chairTarget);
        }

        public bool Interact(Interactor interactor)
        {
            if (interactor.TryGetComponent(out Player.Controller player))
            {
                StartCoroutine(RotateAndDetachHead(player, _target));
            }
            return true;
        }

        public void AddOutline()
        {
        }

        public void RemoveOutline()
        {
        }


        public void Restart()
        {
        }
    }
}
