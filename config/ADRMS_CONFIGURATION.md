# Configuración de AD RMS

## Archivo de Configuración

El archivo `config/adrms.json` se copia automáticamente al directorio de salida del programa (`bin\Debug\net472\config\adrms.json` o `bin\Release\net472\config\adrms.json`).

## Estructura del Archivo

```json
{
	"serverUrl": "https://tu-servidor-rms.com/_wmcs/Certification",
	"extranetUrl": "https://tu-servidor-rms.com/_wmcs/Certification",
	"intranetUrl": "https://tu-servidor-rms.com/_wmcs/Certification",
	"licensingOnlyClusters": true
}
```

### Campos

- **`serverUrl`** (opcional): URL única del servidor AD RMS. Se usa como fallback si no se especifican `extranetUrl` e `intranetUrl`.
- **`extranetUrl`** (opcional): URL externa del servidor AD RMS para acceso desde fuera de la red corporativa.
- **`intranetUrl`** (opcional): URL interna del servidor AD RMS para acceso desde dentro de la red corporativa.
- **`licensingOnlyClusters`** (opcional, por defecto `true`): Indica si el servidor es un clúster de solo licencias. Mapea a `OverrideServiceDiscoveryForLicensing` en MSIPC.

### Ejemplo de Configuración

#### Servidor único (mismo URL para extranet e intranet):
```json
{
	"serverUrl": "https://rms.contoso.com/_wmcs/Certification",
	"licensingOnlyClusters": false
}
```

#### Servidor con URLs diferentes:
```json
{
	"extranetUrl": "https://rms-extranet.contoso.com/_wmcs/Certification",
	"intranetUrl": "https://rms.contoso.com/_wmcs/Certification",
	"licensingOnlyClusters": false
}
```

#### Clúster de solo licencias:
```json
{
	"serverUrl": "https://rms-licensing.contoso.com/_wmcs/licensing",
	"licensingOnlyClusters": true
}
```

## Cómo Funciona

### Programa Principal (`Msipc.CSharp.WinForms`)

1. Lee el archivo desde `bin\Debug\net472\config\adrms.json` (o `bin\Release\net472\config\adrms.json`)
2. Si encuentra `extranetUrl` e `intranetUrl`, los usa directamente
3. Si no los encuentra pero hay `serverUrl`, usa `serverUrl` para ambos
4. Si no encuentra el archivo o no puede leerlo, muestra un error y no permite proteger archivos

### Tests (`Tests.Msipc.CSharp.WinForms`)

**NOTA:** Actualmente los tests tienen el código comentado y usan valores hardcodeados:
- URL por defecto: `https://localhost/_wmcs/licensing`
- No leen el archivo `config/adrms.json`

Para usar la configuración real en los tests, descomentar el código en `TestHelpers.cs` y agregar referencia a `Newtonsoft.Json`.

## Configuración para Pruebas

1. **Edita `config/adrms.json`** con la URL de tu servidor AD RMS:
   ```json
   {
   	"serverUrl": "https://TU-SERVIDOR-RMS/_wmcs/Certification",
   	"licensingOnlyClusters": false
   }
   ```

2. **Compila el proyecto** - El archivo se copiará automáticamente al directorio de salida

3. **Ejecuta el programa** - Selecciona "AD RMS" como backend en la interfaz

4. **Verifica la conexión** - Intenta obtener templates o proteger un archivo

## Troubleshooting

- **Error: "AD RMS config not found"**: El archivo no está en `bin\Debug\net472\config\adrms.json`. Verifica que se haya copiado durante la compilación.
- **Error de conexión**: Verifica que la URL del servidor sea correcta y accesible desde tu máquina.
- **No se encuentran templates**: Verifica que el servidor AD RMS esté funcionando y que tengas permisos para acceder a los templates.

