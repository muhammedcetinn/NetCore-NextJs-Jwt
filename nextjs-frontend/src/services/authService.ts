import { apiClient } from '@/lib/apiClient';
import { LoginCredentials, UserAuthResponse } from '@/types/auth'; // Tipleri merkezi bir yerden alacağız

/**
 * Kullanıcı giriş işlemini gerçekleştirir.
 */
export const login = (credentials: LoginCredentials): Promise<UserAuthResponse> => {
  return apiClient('/auth/login', {
    method: 'POST',
    body: JSON.stringify(credentials),
  });
};

/**
 * Kullanıcı çıkış işlemini gerçekleştirir.
 */
export const logout = (): Promise<void> => {
  return apiClient('/auth/logout', {
    method: 'POST',
  });
};

/**
 * Mevcut kullanıcının oturum bilgilerini getirir.
 */
export const getCurrentUser = (): Promise<UserAuthResponse> => {
  return apiClient('/auth/me', {
    method: 'GET',
  });
};