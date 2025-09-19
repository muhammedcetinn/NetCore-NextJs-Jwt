import toast from 'react-hot-toast';

// CSRF token'ını saklamak ve yönetmek için bir "singleton" desenine benzer yapı.
// Bu, token'ı global ama kontrollü bir şekilde saklamamızı sağlar.
const csrfTokenManager = {
  token: null as string | null,
  setToken(newToken: string | null) {
    this.token = newToken;
  },
  getToken() {
    return this.token;
  },
};
// Bu manager'ı dışarıya açarak AuthContext gibi yerlerden token'ı set etmesini sağlayacağız.
// Fonksiyonu, 'this' bağlamını kaybetmeyecek şekilde doğrudan export et.
// Bu, fonksiyonun her zaman 'csrfTokenManager' objesi üzerinden çağrılmasını sağlar.
export function setCsrfToken(token: string | null) {
  csrfTokenManager.setToken(token);
}


// API URL'ini ortam değişkeninden alıyoruz.
const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5258/api';

/**
 * Projedeki tüm API istekleri için temel fetch sarmalayıcısı (wrapper).
 * @param endpoint Çağrılacak endpoint (örn: '/auth/login').
 * @param options Standart fetch options objesi (method, body, headers vb.).
 */
export async function apiClient<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const { method = 'GET', body, headers = {} } = options;

  const defaultHeaders: HeadersInit = {
    'Content-Type': 'application/json',
    ...headers,
  };
  
  // CSRF token'ı sadece tehlikeli metodlar için ve varsa ekle.
  const csrfToken = csrfTokenManager.getToken();
  if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(method.toUpperCase()) && csrfToken) {
    (defaultHeaders as Record<string, string>)['X-CSRF-Token'] = csrfToken;
  }
  
  try {
    const response = await fetch(`${API_URL}${endpoint}`, {
      ...options,
      method,
      headers: defaultHeaders,
      // Cookie'lerin gönderilip alınması için bu ayar kritik.
      credentials: 'include',
      body,
    });

    // Cevap başarılı değilse (4xx, 5xx), bir hata fırlat.
    if (!response.ok) {
      await handleApiError(response);
    }

    // Cevap başarılı ve body varsa, JSON olarak parse et.
    // 204 No Content gibi durumlar için body kontrolü yap.
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.indexOf("application/json") !== -1) {
      return await response.json() as T;
    }
    
    // Body yoksa veya JSON değilse, null veya undefined dönebiliriz.
    return undefined as T;

  } catch (error) {
    // Network hatası veya handleApiError'dan fırlatılan hata buraya düşer.
    const status = (error as any)?.status;
    if (status !== 401) {
      console.error(`API Client Error: ${error}`);
    }
    // Hata bildirimini burada merkezi olarak yönetebiliriz.
    if (status !== 401) {
        toast.error(getFriendlyErrorMessage(error));
    }
    // Hatanın, onu çağıran fonksiyona (örn: login) da ulaşmasını sağla.
    throw error;
  }
}

/**
 * API hatalarını işleyen ve okunabilir bir formata çeviren yardımcı fonksiyon.
 */
async function handleApiError(response: Response) {
  let errorData;
  try {
    errorData = await response.json();
  } catch {
    errorData = { message: `Request failed with status: ${response.status}` };
  }
  
  const error = new Error(errorData.message || 'An unknown API error occurred');
  (error as any).status = response.status;
  (error as any).data = errorData;
  
  throw error;
}

/**
 * Hata nesnesini kullanıcı dostu bir mesaja çevirir.
 */
function getFriendlyErrorMessage(error: any): string {
    const status = error?.status;

    if (status === 401) return 'Oturumunuz sona erdi. Lütfen tekrar giriş yapın.';
    if (status === 403) return 'Bu işlemi yapmaya yetkiniz yok.';
    if (status === 404) return 'İstenen kaynak bulunamadı.';
    if (status >= 500) return 'Sunucuda bir hata oluştu. Lütfen daha sonra tekrar deneyin.';
    
    return error.message || 'Bir hata oluştu.';
}