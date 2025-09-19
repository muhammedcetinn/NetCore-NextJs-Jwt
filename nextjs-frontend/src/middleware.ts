import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Ortam değişkenlerinden API URL'ini al. Bu, production ve development için farklı URL'ler kullanmayı kolaylaştırır.
// NEXT_PUBLIC_ öneki, bu değişkenin tarayıcıya ifşa EDİLMEMESİ gerektiğini belirtir.
// Middleware sunucu tarafında çalıştığı için buna ihtiyacımız yok.
const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258/api';

/**
 * Rota konfigürasyonlarını tek bir yerde yönetmek kodu daha temiz hale getirir.
 */
const ROUTE_CONFIG = {
  publicPaths: ['/login', '/register'], // Herkesin erişebileceği yollar
  protectedPaths: {
    // Bu yollara erişim için sadece giriş yapmış olmak yeterlidir.
    user: ['/profile', '/user'],
    // Bu yollara erişim için 'Admin' rolü gereklidir.
    admin: ['/admin'],
  },
};

/**
 * Ana middleware fonksiyonu
 */
export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const hasAuthCookie = request.cookies.has('refreshToken');

  const isPublicPath = ROUTE_CONFIG.publicPaths.some(path => pathname.startsWith(path));
  const isUserPath = ROUTE_CONFIG.protectedPaths.user.some(path => pathname.startsWith(path));
  const isAdminPath = ROUTE_CONFIG.protectedPaths.admin.some(path => pathname.startsWith(path));

  // --- Senaryo 1: Kullanıcı giriş yapmış ve public bir sayfaya (login/register) gitmeye çalışıyor ---
  if (hasAuthCookie && isPublicPath) {
    // Kullanıcıyı ana sayfaya yönlendir.
    return NextResponse.redirect(new URL('/', request.url));
  }

  // --- Senaryo 2: Kullanıcı giriş yapmamış ve korumalı bir sayfaya gitmeye çalışıyor ---
  if (!hasAuthCookie && (isUserPath || isAdminPath)) {
    // Kullanıcıyı, gitmek istediği hedefi belirterek login sayfasına yönlendir.
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('redirect', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // --- Senaryo 3: Kullanıcı giriş yapmış ve Admin'e özel bir sayfaya gitmeye çalışıyor ---
  if (hasAuthCookie && isAdminPath) {
    try {
      const response = await fetch(`${API_URL}/auth/check-auth`, {
        headers: { Cookie: request.cookies.toString() },
      });

      // API'den geçerli bir cevap gelmezse (örn: token süresi dolmuş), login'e yönlendir.
      if (response.status !== 200) {
        return redirectToLogin(request);
      }

      const data: { roles?: string[] } = await response.json();
      const isAdmin = data.roles?.some(role => role.toLowerCase() === 'admin');
      
      // Eğer kullanıcı admin değilse, ana sayfaya yönlendir.
      if (!isAdmin) {
        return NextResponse.redirect(new URL('/', request.url));
      }

    } catch (error) {
      console.error('Middleware API call failed:', error);
      // API'ye ulaşılamazsa, güvenlik açısından login'e yönlendirmek en doğrusu.
      return redirectToLogin(request);
    }
  }

  // Yukarıdaki özel durumların hiçbiri geçerli değilse, isteğin devam etmesine izin ver.
  // (Örn: giriş yapmış kullanıcının /user sayfasına gitmesi, herkesin ana sayfaya gitmesi vb.)
  return NextResponse.next();
}

/**
 * Yönlendirme mantığını tekrar etmemek için yardımcı fonksiyon.
 */
function redirectToLogin(request: NextRequest): NextResponse {
  const loginUrl = new URL('/login', request.url);
  loginUrl.searchParams.set('redirect', request.nextUrl.pathname);
  return NextResponse.redirect(loginUrl);
}

/**
 * Middleware'in hangi rotalarda çalışacağını belirten config.
 * Bu, gereksiz çalıştırmaları önleyerek performansı artırır.
 */
export const config = {
  matcher: [
    // Public yollar
    '/login',
    '/register',
    // Korumalı user yolları
    '/profile/:path*',
    '/user/:path*',
    // Korumalı admin yolları
    '/admin/:path*',
  ],
};