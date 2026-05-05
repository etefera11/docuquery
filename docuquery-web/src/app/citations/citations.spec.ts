import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Citations } from './citations';

describe('Citations', () => {
  let component: Citations;
  let fixture: ComponentFixture<Citations>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Citations],
    }).compileComponents();

    fixture = TestBed.createComponent(Citations);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
