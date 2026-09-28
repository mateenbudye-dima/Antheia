// ==========================================
// 1. PUBLIC UI COMPONENTS
// ==========================================
// Only export top-level page or container components that other routes/pages need.
export { BlendEditor } from './components/BlendEditor';

// Optional: Export list/view components if they are consumed externally
// export { BlendList } from './components/BlendList';


// ==========================================
// 2. PUBLIC TYPES & DTOs
// ==========================================
// Export interfaces that external components, routers, or global state need.
export * from './types/blend.types';


// ==========================================
// 3. PUBLIC API SERVICES (Optional)
// ==========================================
// Export API functions if parent pages need to invoke fetching directly.
export { blendsApi } from './api/blendsApi';

// ==========================================
// 4. Pages
// ==========================================
// Export API functions if parent pages need to invoke fetching directly.
export { BlendEditPage } from './pages/BlendEditPage';
export { BlendListPage } from './pages/BlendListPage';