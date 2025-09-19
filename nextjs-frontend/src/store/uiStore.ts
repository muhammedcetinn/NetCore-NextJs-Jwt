import { create } from 'zustand';

/**
 * Uygulama genelindeki UI (Kullanıcı Arayüzü) durumlarını yönetir.
 * Örn: Global yüklenme göstergeleri, modal'ların açık/kapalı durumu vb.
 */

// State'in ve action'ların tipini ayrı ayrı tanımlamak,
// store büyüdükçe daha okunaklı hale gelir.
type State = {
  isApiLoading: boolean;
};

type Actions = {
  setApiLoading: (isLoading: boolean) => void;
  // Gelecekte eklenebilecek diğer action'lar için bir yer:
  // openConfirmationModal: (message: string) => void;
};

// Zustand create fonksiyonuna generic olarak State & Actions'ı veriyoruz.
export const useUiStore = create<State & Actions>((set) => ({
  // --- STATE ---
  isApiLoading: false,

  // --- ACTIONS ---
  setApiLoading: (isLoading) => set({ isApiLoading: isLoading }),
  
  // Örnek: Gelecekte bir onay modal'ı eklemek istediğinizde
  // sadece buraya yeni bir state ve action eklemeniz yeterli olacaktır.
  // confirmationModal: { isOpen: false, message: '' },
  // openConfirmationModal: (message) => set({ confirmationModal: { isOpen: true, message } }),
  // closeConfirmationModal: () => set({ confirmationModal: { isOpen: false, message: '' } }),
}));