"use client";

import React, { createContext, useState, useContext, useEffect, useMemo, useCallback } from 'react';
// Yeni servislerimizi ve tiplerimizi import ediyoruz
import { login as loginService, logout as logoutService, getCurrentUser } from '@/services/authService';
import { setCsrfToken } from '@/lib/apiClient';
import { User, LoginCredentials } from '@/types/auth';

// Context'in dışarıya açacağı değerlerin tipi.
// CSRF token'ını artık dışarıya açmamıza gerek yok, bu bir iç detay.
interface AuthContextType {
  user: User | null;
  isLoading: boolean; // Sadece ilk yükleme için
  login: (credentials: LoginCredentials) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType>({
  user: null,
  isLoading: true,
  login: async () => {},
  logout: async () => {},
});

export const useAuth = () => useContext(AuthContext);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  // Bu isLoading, sadece uygulamanın ilk açılışındaki oturum kontrolü için
  const [isLoadingInitial, setIsLoadingInitial] = useState(true);

  /**
   * Başarılı bir kimlik doğrulama işleminden sonra state'i günceller.
   */
  const handleAuthSuccess = useCallback((data: User & { csrfToken: string }) => {
    // CSRF token'ını apiClient'a bildir
    setCsrfToken(data.csrfToken);
    // User state'ini güncelle
    setUser({ id: data.id, email: data.email, roles: data.roles });
  }, []);

  /**
   * Oturumu sonlandırır ve tüm state'leri temizler.
   */
  const handleLogout = useCallback(() => {
    setCsrfToken(null);
    setUser(null);
  }, []);
  
  /**
   * Uygulama ilk yüklendiğinde çalışarak mevcut oturumu kontrol eder.
   */
  useEffect(() => {
    const checkCurrentUser = async () => {
      try {
        const data = await getCurrentUser();
        handleAuthSuccess(data);
      } catch (error) {
        // Geçerli bir oturum yoksa bu beklenen bir durumdur.
        handleLogout();
      } finally {
        setIsLoadingInitial(false);
      }
    };
    checkCurrentUser();
    // Bu hook'un sadece bir kez çalışması kritik
  }, [handleAuthSuccess, handleLogout]);

  /**
   * Kullanıcı giriş işlemini yönetir.
   */
  const login = useCallback(async (credentials: LoginCredentials) => {
    try {
      const data = await loginService(credentials);
      handleAuthSuccess(data);
    } catch (error) {
      // Hata bildirimi (toast) zaten apiClient katmanında yapılıyor.
      // Sadece hatayı yeniden fırlatarak, çağıran component'in (örn: LoginPage)
      // kendi içinde ek bir işlem yapmasına (örn: setError state'ini güncelleme) izin veriyoruz.
      handleLogout(); // Başarısız login sonrası state'i temizle
      throw error;
    }
  }, [handleAuthSuccess, handleLogout]);

  /**
   * Kullanıcı çıkış işlemini yönetir.
   */
  const logout = useCallback(async () => {
    try {
      await logoutService();
    } catch (error) {
      // Hata bildirimi (toast) zaten apiClient katmanında yapılıyor.
      console.error('Logout request failed, but clearing client state anyway.', error);
    } finally {
      // API isteği başarısız olsa bile client tarafındaki oturumu sonlandır.
      handleLogout();
    }
  }, [handleLogout]);

  const value = useMemo(() => ({
    user,
    isLoading: isLoadingInitial,
    login,
    logout
  }), [user, isLoadingInitial, login, logout]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};