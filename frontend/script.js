const form = document.querySelector("#registration-form");
const fullName = document.querySelector("#full-name");
const studentEmail = document.querySelector("#student-email");
const eventChoice = document.querySelector("#event-choice");
const formMessage = document.querySelector("#form-message");
const submitButton = form.querySelector("[type='submit']");
const emailPattern = /^[^\s@]+@univ\.edu\.ph$/i;

const fields = [fullName, studentEmail, eventChoice];

function setMessage(message, isError = false) {
  formMessage.textContent = message;
  formMessage.classList.toggle("is-error", isError);
}

function getInvalidField() {
  for (const field of fields) {
    const value = field.value.trim();

    if (!value) {
      return { field, message: "Please complete your name, student email, and event selection." };
    }

    if (field === studentEmail && (!field.validity.valid || !emailPattern.test(value))) {
      return { field, message: "Enter a valid email address ending in @univ.edu.ph." };
    }
  }

  return null;
}

for (const field of fields) {
  field.addEventListener("input", () => {
    field.removeAttribute("aria-invalid");
    setMessage("");
  });

  field.addEventListener("change", () => {
    field.removeAttribute("aria-invalid");
    setMessage("");
  });
}

document.querySelectorAll(".choose-event").forEach((button) => {
  button.addEventListener("click", () => {
    eventChoice.value = button.dataset.event;
    eventChoice.removeAttribute("aria-invalid");
    setMessage(`${button.dataset.event} selected.`);
    document.querySelector("#registration").scrollIntoView({ behavior: "smooth" });
    eventChoice.focus({ preventScroll: true });
  });
});

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  const invalid = getInvalidField();
  fields.forEach((field) => field.removeAttribute("aria-invalid"));

  if (invalid) {
    invalid.field.setAttribute("aria-invalid", "true");
    setMessage(invalid.message, true);
    invalid.field.focus();
    return;
  }

  submitButton.disabled = true;
  form.setAttribute("aria-busy", "true");

  try {
    const response = await fetch("/api/registrations", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        fullName: fullName.value.trim(),
        email: studentEmail.value.trim(),
        eventTitle: eventChoice.value,
      }),
    });
    const result = await response.json().catch(() => ({}));

    if (!response.ok) {
      const validationMessage = Object.values(result.errors ?? {}).flat()[0];
      setMessage(result.message ?? result.detail ?? validationMessage ?? "Registration could not be completed. Please try again.", true);
      return;
    }

    setMessage(`Registration confirmed for ${result.eventTitle}. Reference: ${result.registrationId}.`);
  } catch {
    setMessage("Could not reach the registration server. Start the backend and try again.", true);
  } finally {
    submitButton.disabled = false;
    form.removeAttribute("aria-busy");
  }
});