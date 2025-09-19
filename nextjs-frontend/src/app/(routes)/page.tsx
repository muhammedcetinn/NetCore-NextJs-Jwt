"use client";

import { useAuth } from "@/context/AuthContext";
import InfoBox from "@/components/ui/InfoBox/InfoBox"; // Yeni InfoBox component'i
import { useRoles } from "@/hooks/useRoles"; // Yeni rol hook'u (isteğe bağlı)

export default function HomePage() {
  // isLoading'i AuthContext'ten, rol bilgilerini ise useRoles'dan alıyoruz.
  // Bu, sorumlulukları ayırır.
  const { isLoading } = useAuth();
  const { user, isAdmin } = useRoles();

  // İlk yükleme sırasında bir bekleme ekranı göster
  if (isLoading) {
    return <div>Oturum bilgileri yükleniyor...</div>;
  }

  return (
    <section>
      <h1>Ana Sayfa</h1>
      <p>Bu sayfada, kullanıcının rolüne göre farklı içerikler gösterilmektedir.</p>

      <InfoBox title="Herkesin Gördüğü Alan" variant="default">
        <p>Bu bölümü hem misafirler hem de giriş yapmış tüm kullanıcılar görebilir.</p>
      </InfoBox>

      {/* Sadece giriş yapmış bir kullanıcı varsa bu kutuyu göster. */}
      {user && (
        <InfoBox title="Sadece Kullanıcıların Gördüğü Alan" variant="primary">
          <p>Bu bölümü sadece giriş yapmış kullanıcılar (User ve Admin rolleri) görebilir.</p>
          <p>Kullanıcı E-postanız: <strong>{user.email}</strong></p>
        </InfoBox>
      )}

      {/* Sadece admin rolüne sahip kullanıcılar bu kutuyu görebilir. */}
      {isAdmin && (
        <InfoBox title="Sadece Admin'in Gördüğü Alan" variant="danger">
            <p>Bu özel bölümü SADECE <strong>Admin</strong> rolüne sahip kullanıcılar görebilir.</p>
        </InfoBox>
      )}
    </section>
  );
}