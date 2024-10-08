using Unity.Entities;
using UnityEngine;

namespace QTS.QWorld.System
{
    // public class PlayerHybridSystem : MonoBehaviour
    // {
    //     private World _world;
    //     private EntityManager _entityManager;
    //     private RefRW<PlayerComponent> _playerComponent;
    //     private EntityQuery _query;

    //     private void Start()
    //     {
    //         _world = World.DefaultGameObjectInjectionWorld;
    //         _entityManager = _world.EntityManager;

    //         _query = _entityManager.CreateEntityQuery(typeof(PlayerComponent));

    //     }

    //     private void FixedUpdate()
    //     {
    //         if (_query.TryGetSingletonRW<PlayerComponent>(out _playerComponent))
    //         {
    //             _playerComponent.ValueRW.position = transform.position;
    //         }
    //     }
    // }
}