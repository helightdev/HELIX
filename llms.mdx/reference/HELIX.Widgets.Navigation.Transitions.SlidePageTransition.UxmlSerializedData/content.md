# SlidePageTransition.UxmlSerializedData (/reference/HELIX.Widgets.Navigation.Transitions.SlidePageTransition.UxmlSerializedData)

# SlidePageTransition.UxmlSerializedData

```
[Serializable]
public class SlidePageTransition.UxmlSerializedData : PageTransition.UxmlSerializedData
```

## Register()

```
[RegisterUxmlCache]
[Conditional("UNITY_EDITOR")]
public static void Register()
```

## CreateInstance()

```
public override object CreateInstance()
```

<p>
Returns an instance of the declaring element.
</p>

## Deserialize(object)

```
public override void Deserialize(object obj)
```

<p>
Applies serialized field values to a compatible visual element.
</p>