import {createRoot} from 'react-dom/client';
import App from './App.tsx';
import './index.css';

// Shield against external browser extension injected script errors
if (typeof window !== 'undefined') {
  window.addEventListener('error', (event) => {
    if (
      event.filename?.includes('chrome-extension://') ||
      /metamask/i.test(event.message || '')
    ) {
      event.stopImmediatePropagation();
      event.preventDefault();
    }
  });

  window.addEventListener('unhandledrejection', (event) => {
    const reason = event.reason?.message || String(event.reason || '');
    const stack = event.reason?.stack || '';
    if (
      /metamask/i.test(reason) ||
      stack.includes('chrome-extension://') ||
      stack.includes('inpage.js')
    ) {
      event.stopImmediatePropagation();
      event.preventDefault();
    }
  });
}

createRoot(document.getElementById('root')!).render(<App />);
