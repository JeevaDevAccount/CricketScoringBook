import { Navigate, Route, Routes } from "react-router-dom";
import { MatchListPage } from "./pages/MatchListPage";

function App() {
  return (
    <Routes>
      <Route path="/matches" element={<MatchListPage />} />

      <Route
        path="*"
        element={<Navigate to="/matches" replace />}
      />
    </Routes>
  );
}

export default App;