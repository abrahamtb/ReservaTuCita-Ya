import { useEffect } from "react";
import "./Toast.css";

interface Props {
  mensaje: string;
  tipo?: "exito" | "error" | "info";
  onClose: () => void;
}

export function Toast({ mensaje, tipo = "info", onClose }: Props) {
  useEffect(() => {
    const timer = setTimeout(onClose, 3000); // se cierra a los 3 s
    return () => clearTimeout(timer);
  }, [onClose]);

  return (
    <div className={`toast toast-${tipo}`}>
      <span>{mensaje}</span>
      <button onClick={onClose}>×</button>
    </div>
  );
}
