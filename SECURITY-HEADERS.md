# Cabeceras de Seguridad HTTP

Este documento explica las cabeceras de seguridad implementadas en la aplicación para proteger contra vulnerabilidades comunes.

## Cabeceras Implementadas

### 1. Strict-Transport-Security (HSTS)
```
Strict-Transport-Security: max-age=31536000; includeSubDomains
```
- **Propósito**: Fuerza conexiones HTTPS durante 1 año
- **Protege contra**: Ataques man-in-the-middle, downgrade attacks
- **Calificación**: A+

### 2. Content-Security-Policy (CSP)
```
Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; ...
```
- **Propósito**: Previene ataques XSS controlando qué recursos puede cargar el navegador
- **Protege contra**: Cross-Site Scripting (XSS), inyección de código malicioso
- **Nota**: Se permite `unsafe-inline` para Bootstrap y SweetAlert2

### 3. X-Frame-Options
```
X-Frame-Options: SAMEORIGIN
```
- **Propósito**: Previene que el sitio sea embebido en iframes de otros dominios
- **Protege contra**: Clickjacking

### 4. X-Content-Type-Options
```
X-Content-Type-Options: nosniff
```
- **Propósito**: Previene que el navegador "adivine" el tipo de contenido
- **Protege contra**: MIME sniffing attacks

### 5. Referrer-Policy
```
Referrer-Policy: strict-origin-when-cross-origin
```
- **Propósito**: Controla qué información de referencia se envía en las navegaciones
- **Protege contra**: Fuga de información sensible en URLs

### 6. Permissions-Policy
```
Permissions-Policy: geolocation=(), microphone=(), camera=(), payment=()
```
- **Propósito**: Deshabilita APIs del navegador que no se utilizan
- **Protege contra**: Uso no autorizado de características del dispositivo

### 7. Cross-Origin-Embedder-Policy (COEP)
```
Cross-Origin-Embedder-Policy: require-corp
```
- **Propósito**: Aísla recursos de origen cruzado
- **Protege contra**: Ataques Spectre y aislamiento de origen

### 8. Cross-Origin-Opener-Policy (COOP)
```
Cross-Origin-Opener-Policy: same-origin
```
- **Propósito**: Previene que otros documentos compartan el contexto de navegación
- **Protege contra**: Ataques de timing cross-origin

### 9. Cross-Origin-Resource-Policy (CORP)
```
Cross-Origin-Resource-Policy: same-origin
```
- **Propósito**: Controla quién puede cargar los recursos
- **Protege contra**: Fuga de información cross-site

### 10. X-XSS-Protection (Legacy)
```
X-XSS-Protection: 1; mode=block
```
- **Propósito**: Habilita filtro XSS del navegador (legacy)
- **Nota**: Reemplazado por CSP en navegadores modernos

## Verificación

Para verificar las cabeceras de seguridad:

1. **SecurityHeaders.com**
   ```
   https://securityheaders.com/?q=https://uned-ecmar.onrender.com
   ```

2. **Mozilla Observatory**
   ```
   https://observatory.mozilla.org/
   ```

3. **Curl (línea de comandos)**
   ```bash
   curl -I https://uned-ecmar.onrender.com/Login
   ```

## Resultado Esperado

Con estas cabeceras implementadas, la aplicación debería obtener una calificación de **A** o **A+** en SecurityHeaders.com.

## Notas Importantes

- Las cabeceras se aplican a todas las respuestas HTTP mediante middleware
- Se eliminan las cabeceras `Server` y `X-Powered-By` para no exponer información del servidor
- HSTS está configurado con preload para máxima seguridad
- CSP permite recursos de CDN (Bootstrap, SweetAlert2) necesarios para la aplicación

## Referencias

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [MDN Web Security](https://developer.mozilla.org/en-US/docs/Web/Security)
- [SecurityHeaders.com](https://securityheaders.com/)
