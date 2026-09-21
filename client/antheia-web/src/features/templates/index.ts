// ==========================================
// 1. PUBLIC UI COMPONENTS
// ==========================================
// Only export top-level page or container components that other routes/pages need.
export { TemplateEditor } from './components/TemplateEditor';

// Optional: Export list/view components if they are consumed externally
// export { TemplateList } from './components/TemplateList';


// ==========================================
// 2. PUBLIC TYPES & DTOs
// ==========================================
// Export interfaces that external components, routers, or global state need.
export * from './types/template.types';


// ==========================================
// 3. PUBLIC API SERVICES (Optional)
// ==========================================
// Export API functions if parent pages need to invoke fetching directly.
export { templatesApi } from './api/templatesApi';

// ==========================================
// 4. Pages
// ==========================================
// Export API functions if parent pages need to invoke fetching directly.
export { TemplateEditPage } from './pages/TemplateEditPage';
export { TemplateListPage } from './pages/TemplateListPage';