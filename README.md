# Actividad5Firebase

## Nota importante para clonar el repositorio

Este proyecto utiliza Firebase para Unity. Por límite de tamaño de GitHub, no se incluyeron algunos archivos nativos de Firebase que superan los 100 MB:

```text
Assets/Firebase/Plugins/x86_64/FirebaseCppApp-13_10_0.bundle
Assets/Firebase/Plugins/x86_64/FirebaseCppApp-13_10_0.so
```

Estos archivos fueron agregados al `.gitignore`, por lo que no aparecerán al clonar el repositorio.

## Guía para restaurar los archivos faltantes

Después de clonar el repositorio, se deben volver a importar los paquetes de Firebase en Unity.

Pasos:

1. Descargar el **Firebase Unity SDK** desde la página oficial de Firebase.

2. Descomprimir el archivo descargado.

3. Abrir el proyecto en Unity.

4. Ir a:

```text
Assets > Import Package > Custom Package
```

5. Desde la carpeta `dotnet4` del Firebase Unity SDK, importar:

```text
FirebaseAuth.unitypackage
FirebaseDatabase.unitypackage
```

6. Aceptar la resolución de dependencias de Android si Unity lo solicita.

7. Verificar que exista el archivo:

```text
Assets/google-services.json
```

8. Abrir la escena principal y ejecutar el proyecto desde el editor.

## Nota

Si Unity muestra errores relacionados con Firebase después de clonar el repositorio, normalmente se solucionan importando nuevamente `FirebaseAuth.unitypackage` y `FirebaseDatabase.unitypackage`.
