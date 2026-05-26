import { useState, useCallback } from "react";
import { useDropzone } from "react-dropzone";
import axios from "axios";
import "./App.css";

const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5000";

interface FileState {
  file: File | null;
  status: "idle" | "uploading" | "converting" | "success" | "error";
  message: string;
}

function App() {
  const [fileState, setFileState] = useState<FileState>({
    file: null,
    status: "idle",
    message: "",
  });

  const onDrop = useCallback((acceptedFiles: File[]) => {
    if (acceptedFiles.length > 0) {
      const file = acceptedFiles[0];
      if (!file.name.toLowerCase().endsWith(".docx")) {
        setFileState({ file: null, status: "error", message: "Only .docx files are supported" });
        return;
      }
      setFileState({ file, status: "idle", message: "" });
    }
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "application/vnd.openxmlformats-officedocument.wordprocessingml.document": [".docx"],
    },
    maxFiles: 1,
    multiple: false,
  });

  const handleConvert = async () => {
    if (!fileState.file) return;

    setFileState((prev) => ({ ...prev, status: "uploading", message: "Uploading..." }));

    const formData = new FormData();
    formData.append("file", fileState.file);

    try {
      const response = await axios.post(`${API_URL}/api/convert`, formData, {
        responseType: "blob",
        onUploadProgress: (progressEvent) => {
          if (progressEvent.total) {
            const percent = Math.round((progressEvent.loaded * 100) / progressEvent.total);
            setFileState((prev) => ({
              ...prev,
              status: "uploading",
              message: `Uploading... ${percent}%`,
            }));
          }
        },
      });

      setFileState((prev) => ({ ...prev, status: "success", message: "Conversion complete!" }));

      // Auto-download
      const blob = new Blob([response.data], { type: "application/pdf" });
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileState.file.name.replace(/\.docx$/i, ".pdf");
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      let message = "Conversion failed";
      if (axios.isAxiosError(error) && error.response?.data instanceof Blob) {
        try {
          const text = await error.response.data.text();
          const parsed = JSON.parse(text);
          message = parsed.message || parsed.detail || message;
        } catch {}
      }
      setFileState((prev) => ({ ...prev, status: "error", message }));
    }
  };

  const handleReset = () => {
    setFileState({ file: null, status: "idle", message: "" });
  };

  return (
    <div className="app">
      <header className="header">
        <h1>Docx to PDF</h1>
        <p className="subtitle">Convert your Word documents to PDF</p>
      </header>

      <main className="main">
        {!fileState.file ? (
          <div {...getRootProps()} className={`dropzone ${isDragActive ? "active" : ""}`}>
            <input {...getInputProps()} />
            {isDragActive ? (
              <p>Drop your DOCX file here...</p>
            ) : (
              <div className="dropzone-content">
                <div className="icon">📄</div>
                <p>Drag & drop your DOCX file here</p>
                <p className="hint">or click to browse</p>
              </div>
            )}
          </div>
        ) : (
          <div className="file-card">
            <div className="file-info">
              <span className="file-icon">📄</span>
              <div className="file-details">
                <p className="file-name">{fileState.file.name}</p>
                <p className="file-size">
                  {(fileState.file.size / 1024 / 1024).toFixed(2)} MB
                </p>
              </div>
              <button className="btn-reset" onClick={handleReset} disabled={fileState.status === "uploading"}>
                ✕
              </button>
            </div>

            {fileState.status === "idle" && (
              <button className="btn-convert" onClick={handleConvert}>
                Convert to PDF
              </button>
            )}

            {(fileState.status === "uploading" || fileState.status === "converting") && (
              <div className="progress">
                <div className="spinner"></div>
                <p>{fileState.message}</p>
              </div>
            )}

            {fileState.status === "success" && (
              <div className="success-message">
                <span>✅</span> {fileState.message}
              </div>
            )}

            {fileState.status === "error" && (
              <div className="error-message">
                <span>❌</span> {fileState.message}
                <button className="btn-retry" onClick={handleConvert}>
                  Retry
                </button>
              </div>
            )}
          </div>
        )}
      </main>
    </div>
  );
}

export default App;
