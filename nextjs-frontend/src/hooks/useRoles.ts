"use client";

import { useAuth } from "@/context/AuthContext";

/**
 * Mevcut kullanıcının rollerini kontrol etmek için bir custom hook.
 * Rol kontrol mantığını merkezileştirir ve yeniden kullanılabilir hale getirir.
 */
export function useRoles() {
  const { user } = useAuth();

  const checkRole = (roleName: string) => {
    return user?.roles.some(role => role.toLowerCase() === roleName.toLowerCase()) ?? false;
  };

  return {
    user, // Orijinal user objesini de döndürmek faydalı olabilir
    isAdmin: checkRole('admin'),
    isUser: checkRole('user'), // İhtiyaç duyulursa diye ekleyebiliriz
    // Gelecekte eklenebilecek diğer roller:
    // isModerator: checkRole('moderator'),
  };
}