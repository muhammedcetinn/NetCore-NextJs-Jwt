# Proje: .NET 8 & Next.js ile Modern Kimlik Doğrulama ve Yetkilendirme

Bu proje, .NET 8 Web API backend'i ve Next.js (App Router) frontend'i kullanarak modern, güvenli ve özellik zengini bir kimlik doğrulama (authentication) ve rol bazlı yetkilendirme (authorization) sisteminin nasıl inşa edileceğini gösteren kapsamlı bir örnektir.

## Projenin Amacı

Bu projenin temel amacı, endüstri standardı en iyi pratikleri (best practices) takip ederek, aşağıdaki özelliklere sahip, production'a hazır bir altyapı oluşturmaktır:

-   **Güvenlik Odaklı Mimari:** `httpOnly` cookie'ler, CSRF koruması ve Refresh Token Rotasyonu gibi mekanizmalarla en üst düzeyde güvenlik sağlamak.
-   **Katmanlı ve Sürdürülebilir Kod:** Hem backend hem de frontend'de sorumlulukları net bir şekilde ayıran (Controller, Service, Repository, Hooks, API Katmanı vb.) bir yapı kurmak.
-   **Modern Frontend Deneyimi:** Next.js'in App Router'ını kullanarak, global state yönetimi, iskelet yükleyiciler (skeleton loaders) ve anlık geri bildirimler (toast notifications) ile akıcı bir kullanıcı deneyimi sunmak.
-   **Esnek ve Yapılandırılabilir Backend:** .NET'in güçlü Bağımlılık Enjeksiyonu (Dependency Injection) ve yapılandırma (configuration) modellerini kullanarak esnek ve yönetilebilir bir API oluşturmak.

---

## Mimarî ve Teknoloji Yığını

### Backend (`net-core-api-backend`)

-   **Framework:** .NET 8 Web API
-   **Kimlik Yönetimi:** ASP.NET Core Identity
-   **Veritabanı:** Entity Framework Core 8 (SQL Server ile)
-   **Kimlik Doğrulama:** JWT (JSON Web Tokens)
-   **Mimari:** Katmanlı Mimari (Controllers, Services, Extensions, Middleware)

### Frontend (`nextjs-frontend`)

-   **Framework:** Next.js 14+ (App Router)
-   **Dil:** TypeScript
-   **API İletişimi:** Native `fetch` API üzerine kurulu merkezi bir API istemcisi (`apiClient`)
-   **State Yönetimi:** React Context API (`AuthContext`) ve Zustand (Global UI State için)
-   **Stil:** CSS Modules
-   **UI/UX:** `react-hot-toast` (bildirimler), iskelet yükleyiciler

---

## Kurulum ve Başlatma

### Ön Gereksinimler

-   .NET 8 SDK
-   Node.js v18+
-   SQL Server (veya başka bir veritabanı ve ilgili EF Core provider'ı)

### Backend Kurulumu

1.  **Bağımlılıkları Yükleyin:** `net-core-api-backend` klasörünün içindeyken terminali açın.
    ```bash
    dotnet restore
    ```
2.  **Veritabanı Ayarları:** `appsettings.Development.json` dosyasındaki `ConnectionStrings.DefaultConnection` alanını kendi SQL Server yapılandırmanıza göre güncelleyin.
3.  **Veritabanını Oluşturun:** Proje dizininde aşağıdaki komutları çalıştırarak veritabanını oluşturun ve başlangıç verilerini (roller ve kullanıcılar) ekleyin.
    ```bash
    dotnet ef database update
    ```
4.  **Uygulamayı Başlatın:**
    ```bash
    dotnet run
    ```
    API, `http://localhost:5258` ve `https://localhost:7258` adreslerinde çalışmaya başlayacaktır.

### Frontend Kurulumu

1.  **Bağımlılıkları Yükleyin:** `nextjs-frontend` klasörünün içindeyken terminali açın.
    ```bash
    npm install
    ```
2.  **Ortam Değişkenleri:** Proje kök dizininde `.env.local` adında bir dosya oluşturun ve backend API'nizin adresini belirtin.
    ```
    NEXT_PUBLIC_API_URL=http://localhost:5258/api
    ```
3.  **Uygulamayı Başlatın:**
    ```bash
    npm run dev
    ```
    Frontend uygulaması, `http://localhost:3000` adresinde çalışmaya başlayacaktır.

---

## Temel Özellikler ve Uygulanan Konseptler

### 1. Güvenli Oturum Yönetimi (`httpOnly` Cookies)

-   **Neden?** Token'ları `localStorage`'da saklamak, XSS (Cross-Site Scripting) saldırılarına karşı savunmasızdır. `httpOnly` cookie'ler, JavaScript'in erişimini engelleyerek bu zafiyeti ortadan kaldırır.
-   **Nasıl?** Backend, `login` ve `refresh` işlemlerinde `accessToken` ve `refreshToken`'ı `HttpOnly`, `Secure` ve `SameSite=Strict` olarak ayarlanmış iki ayrı cookie'ye yazar. Frontend, `credentials: 'include'` ayarı sayesinde bu cookie'leri her istekte otomatik olarak gönderir. Frontend kodu token'ları asla görmez veya yönetmez.

### 2. Refresh Token Rotasyonu ve Yeniden Kullanım Tespiti

-   **Neden?** Bir `refreshToken` çalınırsa, saldırganın süresiz olarak yeni `accessToken`'lar üretmesini engellemek için.
-   **Nasıl?** Bir `refreshToken` her kullanıldığında, backend veritabanında bu token'ı geçersiz kılar ve yerine yenisini üretir. Eğer geçersiz kılınmış bir token tekrar kullanılmaya çalışılırsa, bu bir güvenlik ihlali olarak algılanır ve ilgili kullanıcının tüm oturumları sonlandırılabilir. Bizim implementasyonumuzda, geçersiz token ile yapılan istek reddedilir.

### 3. CSRF (Cross-Site Request Forgery) Koruması

-   **Neden?** Kullanıcının oturumunu kullanarak, başka bir kötü niyetli siteden, kullanıcının haberi olmadan bizim sitemize istek atılmasını engellemek için.
-   **Nasıl?** "Double Submit Cookie" paterni uygulanmıştır.
    1.  Backend, oturum başladığında hem `httpOnly` bir `csrf-token` cookie'si set eder hem de aynı token değerini API cevabında döner.
    2.  Frontend, bu token'ı state'inde saklar.
    3.  `POST`, `PUT`, `DELETE` gibi tehlikeli her istekte, state'deki bu token'ı `X-CSRF-Token` başlığına ekler.
    4.  Backend'deki `CsrfMiddleware`, cookie'deki token ile başlıktaki token'ı karşılaştırır. Eşleşmezse, isteği `403 Forbidden` hatasıyla reddeder.

### 4. Rol Bazlı Yetkilendirme (RBAC)

-   **Neden?** Uygulamanın farklı bölümlerine sadece belirli yetkilere (rollere) sahip kullanıcıların erişebilmesini sağlamak için.
-   **Nasıl?**
    1.  **Backend:** Kullanıcının rolleri (`Admin`, `User`), `accessToken`'ın (JWT) içine bir "claim" olarak eklenir.
    2.  **Frontend (Middleware):** `/admin` gibi hassas bir yola istek geldiğinde, `middleware.ts` bu isteği yakalar. Backend'deki `/api/auth/check-auth` endpoint'ine bir "yetki kontrol" isteği atar. Backend, cookie'den kullanıcıyı ve rollerini anlar. Middleware, backend'den gelen "bu kullanıcı Admin'dir" veya "değildir" cevabına göre kullanıcıyı ya sayfaya devam ettirir ya da başka bir sayfaya yönlendirir.
    3.  **Frontend (UI):** `useRoles` gibi custom hook'lar, `AuthContext`'ten gelen kullanıcı bilgisine göre UI elemanlarını (örn: Navbar'daki "Admin Panel" linki) koşullu olarak render eder.

### 5. Katmanlı ve Organize Kod Mimarisi

-   **Backend:**
    -   **Extensions:** `Program.cs`'i temiz tutmak için servis ve middleware yapılandırmaları (`AuthServiceExtensions`, `CORSPolicyExtensions`) extension metotlarına taşındı.
    -   **Service Katmanı:** Tüm iş mantığı (`login`, `logout` vb.), `AuthController`'dan alınıp test edilebilir ve yeniden kullanılabilir olan `AuthService`'e taşındı. Controller'lar artık "zayıf" ve sadece HTTP isteklerini yönlendirmekle görevli.
-   **Frontend:**
    -   **API Katmanı:** Tüm `fetch` işlemleri, merkezi bir `lib/apiClient.ts` dosyası ve amaca yönelik `services/authService.ts` gibi servis dosyaları altında toplandı. Component'ler artık doğrudan `fetch` çağırmıyor.
    -   **State Yönetimi:** `AuthContext`, sadece kimlik doğrulama state'ini yönetirken, `Zustand` (`uiStore`) global UI state'lerini (örn: `isApiLoading`) yönetiyor. Bu, sorumlulukları ayırır.
    -   **Custom Hooks:** Rol kontrolü gibi tekrarlanan mantıklar, `hooks/useRoles.ts` gibi özel hook'lara taşındı.

### 6. Gelişmiş Kullanıcı Deneyimi (UX)

-   **"Beni Hatırla":** Kullanıcının oturum kalıcılığını (session veya persistent cookie) seçmesine olanak tanır.
-   **Akıllı Yönlendirme:** Korunan bir sayfaya gitmeye çalışan kullanıcı, login olduktan sonra başlangıçta gitmek istediği sayfaya otomatik olarak yönlendirilir.
-   **Global Geri Bildirim:** Bir API isteği sırasında tüm ekranı kaplayan bir `GlobalSpinner` gösterilir. API hataları, `react-hot-toast` ile kullanıcı dostu bildirimler olarak gösterilir.
-   **İskelet Yükleyiciler (Skeleton Loaders):** Sayfalar, oturum bilgisi yüklenirken boş bir ekran yerine sayfanın "iskeletini" göstererek algılanan performansı artırır.

---

## Başlangıç Kullanıcı Bilgileri

Veritabanı, `SeedIdentityData` migration'ı ile aşağıdaki test kullanıcılarını otomatik olarak oluşturur:

-   **Admin Kullanıcısı:**
    -   **E-posta:** `admin@example.com`
    -   **Parola:** `AdminPassword123!`
-   **Standart Kullanıcı:**
    -   **E-posta:** `user@example.com`
    -   **Parola:** `UserPassword123!`
