import { useEffect, useRef, useState } from 'react';
import { useDebounce } from './useDebounce';

export type SaveStatus = 'idle' | 'saving' | 'saved' | 'error';

interface UseAutoSaveOptions<T> {
  /** The value/object to watch and auto-save */
  value: T;
  /** Async function that sends the PATCH/PUT API call */
  onSave: (debouncedValue: T) => Promise<void>;
  /** Delay in milliseconds before firing the save function. Defaults to 800ms. */
  delay?: number;
}

export function useAutoSave<T>({ value, onSave, delay = 800 }: UseAutoSaveOptions<T>) {
  const debouncedValue = useDebounce(value, delay);
  const isInitialRender = useRef(true);
  const [status, setStatus] = useState<SaveStatus>('idle');
  const [error, setError] = useState<Error | null>(null);

  // 1. Keep a ref to the latest onSave callback to prevent stale closures
  const onSaveRef = useRef(onSave);

  // 2. Sync ref whenever onSave changes
  useEffect(() => {
    onSaveRef.current = onSave;
  }, [onSave]);

  useEffect(() => {
    // 3. Skip saving on initial mount
    if (isInitialRender.current) {
      isInitialRender.current = false;
      return;
    }

    let isMounted = true;

    const executeSave = async () => {
      setStatus('saving');
      setError(null);

      try {
        // 4. Call the ref's current function instead of the raw parameter
        await onSaveRef.current(debouncedValue);
        if (isMounted) {
          setStatus('saved');
        }
      } catch (err) {
        console.error('Auto-save error:', err);
        if (isMounted) {
          setError(err instanceof Error ? err : new Error('Auto-save failed'));
          setStatus('error');
        }
      }
    };

    executeSave();

    // 5. Cleanup to handle unmounting mid-request safely
    return () => {
      isMounted = false;
    };
  }, [debouncedValue]); // Safe from missing dependency warnings

  return { status, error };
}