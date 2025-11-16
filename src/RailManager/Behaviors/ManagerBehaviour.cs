using System.Diagnostics.CodeAnalysis;
using RailManager.Features;
using UnityEngine;

namespace RailManager.Behaviors;

[ExcludeFromCodeCoverage]
public class ManagerBehaviour : MonoBehaviour
{
    private static ManagerBehaviour? _Instance;

    private void Awake() {
        if (_Instance != null) {
            return;
        }

        _Instance = this;
        DontDestroyOnLoad(transform.gameObject);
        Bootstrapper.LoadMods();
    }

    private void OnDestroy() {
        if (_Instance == this) {
            _Instance = null;
        }
    }
}
