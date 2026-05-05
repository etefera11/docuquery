import { TestBed } from '@angular/core/testing';

import { DocuQuery } from './docu-query';

describe('DocuQuery', () => {
  let service: DocuQuery;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DocuQuery);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
