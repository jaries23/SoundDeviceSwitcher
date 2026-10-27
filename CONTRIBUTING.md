# 기여 가이드 / Contributing

## 커밋 메시지 규칙 / Commit message convention

이 프로젝트는 [Conventional Commits](https://www.conventionalcommits.org/ko/v1.0.0/)를 따릅니다.
This project follows [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/).

### 형식 / Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

- **type** — 아래 표 참조. 필수. See the table below. Required.
- **scope** — 변경 영역(선택): `audio`, `ui`, `config`, `profiles`, `overlay`, `hotkeys`, `tray`, `updates`, `build` 등. Optional area of change.
- **subject** — 명령형·현재 시제, 끝에 마침표 없이 간결하게. Imperative, present tense, concise, no trailing period.
- **body / footer** — 선택. 이유나 맥락, 주요 변경(BREAKING CHANGE) 등을 적습니다. Optional; explain the why/context or breaking changes.

### 타입 / Types

| type | 용도 / Use | CHANGELOG |
| --- | --- | --- |
| `feat` | 새 기능 / A new feature | Added |
| `fix` | 버그 수정 / A bug fix | Fixed |
| `refactor` | 동작 변화 없는 구조 개선 / Code change that neither fixes a bug nor adds a feature | Changed |
| `perf` | 성능 개선 / Performance improvement | Changed |
| `docs` | 문서만 변경 / Documentation only | — |
| `style` | 포매팅 등 의미 무관 변경 / Formatting, no code-meaning change | — |
| `test` | 테스트 추가·수정 / Tests | — |
| `build` | 빌드·의존성 / Build system or dependencies | — |
| `ci` | CI 설정 / CI configuration | — |
| `chore` | 기타 잡무 / Other maintenance | — |

### 예시 / Examples

```
feat(profiles): add per-app audio routing toggle
fix(tray): restore window when the tray icon is double-clicked
refactor(ui): split MainShellForm into feature-based partial classes
docs: switch release notes to Keep a Changelog format
```

### 주요 변경 / Breaking changes

footer에 `BREAKING CHANGE: <설명>`을 넣거나, type 뒤에 `!`를 붙입니다(예: `feat!: ...`).
Add `BREAKING CHANGE: <description>` in the footer, or append `!` after the type (e.g. `feat!: ...`).

## 릴리스 노트 / Release notes

사용자용 변경 사항은 [CHANGELOG.md](CHANGELOG.md)에 [Keep a Changelog](https://keepachangelog.com/) 형식으로 기록합니다. 커밋의 `feat`/`fix`/`refactor` 등은 다음 버전의 해당 섹션(Added / Fixed / Changed)에 반영하세요.
User-facing changes are recorded in [CHANGELOG.md](CHANGELOG.md) using the [Keep a Changelog](https://keepachangelog.com/) format; land each `feat`/`fix`/`refactor` under the matching section (Added / Fixed / Changed) of the next version.

아직 릴리스되지 않은 변경은 `## [Unreleased]` 아래에 모으고, 릴리스 시 버전 번호와 날짜로 승격합니다.
Collect not-yet-released changes under `## [Unreleased]`, and promote them to a version number and date at release time.

> 이 규칙은 앞으로의 커밋에 적용됩니다. 기존 커밋 히스토리는 재작성하지 않습니다.
> This convention applies to new commits; existing commit history is not rewritten.
