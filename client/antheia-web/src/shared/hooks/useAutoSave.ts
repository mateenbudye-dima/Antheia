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
  const [status, setStatus] = useState<SaveStatus>('idle');
  const [error, setError] = useState<Error | null>(null);

  // 1. Keep a ref to the latest onSave callback to prevent stale closures
  const onSaveRef = useRef(onSave);

  // 2. Track the previous stringified value to guard against initial mount & identical renders
  const lastSavedValueRef = useRef<string>(JSON.stringify(value));

  // 3. Sync ref whenever onSave changes
  useEffect(() => {
    onSaveRef.current = onSave;
  }, [onSave]);

  useEffect(() => {
    const currentStringifiedValue = JSON.stringify(debouncedValue);

    // 4. Skip saving if the debounced value hasn't actually changed from what's already saved
    if (lastSavedValueRef.current === currentStringifiedValue) {
      return;
    }

    let isMounted = true;

    const executeSave = async () => {
      setStatus('saving');
      setError(null);

      try {
        await onSaveRef.current(debouncedValue);
        if (isMounted) {
          // Update the ref so subsequent renders know this value is already persisted
          lastSavedValueRef.current = currentStringifiedValue;
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

    return () => {
      isMounted = false;
    };
  }, [debouncedValue]);

  return { status, error };
}